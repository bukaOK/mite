using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.BLL.DTO;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public sealed class TagsRepository : Repository<Tag>
    {
        public TagsRepository(AppDbContext db) : base(db)
        {
        }
        public Task<IEnumerable<Tag>> GetAllAsync(bool isConfirmed)
        {
            return Db.QueryAsync<Tag>("select * from dbo.\"Tags\" where IsConfirmed=@isConfirmed;", new { isConfirmed });
        }
        public Task<IEnumerable<TagDTO>> GetAllWithPopularityAsync(bool isConfirmed)
        {
            var query = "select dbo.\"Tags\".\"Name\", COUNT(dbo.\"TagPosts\".\"Tag_Id\") as \"Popularity\" "
                + "from dbo.\"Tags\" left outer join dbo.\"TagPosts\" on dbo.\"TagPosts\".\"Tag_Id\"=dbo.\"Tags\".\"Id\"" +
                " where \"IsConfirmed\"=@isConfirmed" 
                + " group by dbo.\"Tags\".\"Name\" order by \"Popularity\" desc;";
            return Db.QueryAsync<TagDTO>(query, new { isConfirmed });
        }
        public async Task<IEnumerable<Tag>> GetByNameAsync(string name)
        {
            var tags = await Table.Where(x => x.Name.Contains(name)).ToListAsync();
            return tags;
        }
        /// <summary>
        /// Исходя из списка постов, получаем теги
        /// </summary>
        /// <param name="postsIds"></param>
        /// <returns></returns>
        public async Task<List<Tag>> GetByPostsAsync(IList<Guid> postsIds)
        {
            //var tags = await Table.Include(x => x.Posts)
            //    .Where(tag => tag.Posts.Any(post => postsIds.Any(postId => postId == post.Id)))
            //    .ToListAsync();
            //return tags;
            var query = "select * from dbo.\"TagPosts\" left outer join"
                + " dbo.\"Tags\" on dbo.\"Tags\".\"Id\"=dbo.\"TagPosts\".\"Tag_Id\" where dbo.\"TagPosts\".\"Post_Id\" = any (@postsIds);";
            var tags = new List<Tag>();
            var rows = await Db.QueryAsync(query, new { postsIds });
            foreach(var row in rows)
            {
                var tag = tags.FirstOrDefault(x => x.Id == row.Id);
                if(tag == null)
                {
                    tag = new Tag
                    {
                        Id = row.Id,
                        Name = row.Name,
                        IsConfirmed = row.IsConfirmed,
                        Posts = new List<Post>()
                    };
                    tags.Add(tag);
                }
                tag.Posts.Add(new Post
                {
                    Id = row.Post_Id
                });
            }
            return tags;
        }
        /// <summary>
        /// Меняем один тег на другой(если например имя старого тега неправильно записано)
        /// </summary>
        /// <returns></returns>
        public async Task BindAsync(Guid oldTagId, Guid newTagId)
        {
            //Получаем Id поста старого тега
            var query = "select \"Post_Id\" from dbo.\"TagPosts\" where \"Tag_Id\"=@oldTagId;";
            var oldTagPostId = await Db.QueryFirstAsync<Guid>(query, new { oldTagId });
            //Смотрим, есть ли уже новый тег в данном посте
            query = "select \"Post_Id\" from dbo.\"TagPosts\" where \"Tag_Id\"=@newTagId and \"Post_Id\"=@oldTagPostId;";
            var newTagResults = await Db.QueryAsync(query, new { newTagId, oldTagPostId });
            //Если есть, его незачем добавлять
            if(newTagResults.Count() == 0)
                query = "update dbo.\"TagPosts\" set \"Tag_Id\" = @newTagId where \"Tag_Id\" = @oldTagId;";
            else
                query = "delete from dbo.\"TagPosts\" where \"Tag_Id\"=@oldTagId;";
            query += "delete from dbo.\"Tags\" where \"Id\"=@oldTagId;";
            await Db.ExecuteAsync(query, new { oldTagId, newTagId });
        }
        public async override Task AddAsync(Tag entity)
        {
            var sameTag = await Table.FirstOrDefaultAsync(x => x.Name == entity.Name);
            if (sameTag != null)
                return;
            Table.Add(entity);
            await SaveAsync();
        }
        /// <summary>
        /// Обновляет теги поста
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task AddWithPostAsync(List<Tag> tags, Guid postId)
        {
            var query = "select * from dbo.\"Tags\" where \"Name\" = any(@Tags); "
                + "select * from dbo.\"TagPosts\" where \"Post_Id\"=@PostId;";
            var multi = await Db.QueryMultipleAsync(query, new { Tags = tags.Select(x => x.Name), PostId = postId });

            var existingTags = await multi.ReadAsync<Tag>();
            var existingTagPosts = await multi.ReadAsync();
            multi.Dispose();

            //Теги для добавления в таблицу тегов
            var tagsToAdd = tags.Where(x => !existingTags.Any(y => y.Name == x.Name));
            foreach(var tag in tagsToAdd)
            {
                tag.Id = Guid.NewGuid();
                tag.Checked = false;
                tag.IsConfirmed = false;
            }
            query = "insert into dbo.\"Tags\" (\"Id\", \"Name\", \"IsConfirmed\", \"Checked\") values (@Id, @Name, @IsConfirmed, @Checked); ";
            await Db.ExecuteAsync(query, tagsToAdd);

            tags = new List<Tag>();
            tags.AddRange(existingTags);
            tags.AddRange(tagsToAdd);

            //Выбираем теги для удаления из постов
            var tagPostsToDel = existingTagPosts.Where(x => !existingTags.Any(y => y.Id == x.Tag_Id))
                .Select(x => x.Tag_Id);
            //Теги для добавления к постам
            var tagPostsToAdd = tags.Where(x => !existingTagPosts.Any(y => y.Tag_Id == x.Id))
                .Select((x => new { TagId = x.Id, PostId = postId }));
            query = "delete from dbo.\"TagPosts\" where \"Tag_Id\" = any(@Tags) and \"Post_Id\"=@PostId;";
            await Db.ExecuteAsync(query, new { Tags = tagPostsToDel, PostId = postId });
            query = "insert into dbo.\"TagPosts\"(\"Tag_Id\", Post_Id) values(@TagId, @PostId);";
            await Db.ExecuteAsync(query, tagPostsToAdd);
        }
    }
}