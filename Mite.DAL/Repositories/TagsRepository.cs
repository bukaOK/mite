using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using Mite.DAL.DTO;

namespace Mite.DAL.Repositories
{
    public sealed class TagsRepository : Repository<Tag>
    {
        public TagsRepository(AppDbContext db) : base(db)
        {
        }
        public async Task<IEnumerable<Tag>> GetAllAsync(bool isConfirmed)
        {
            var tags = await Table.AsNoTracking().Where(x => x.IsConfirmed == isConfirmed).ToListAsync();
            return tags;
        }
        /// <summary>
        /// Получить теги с популярностью
        /// </summary>
        /// <param name="isConfirmed">Подтвержденные/неподтвержденные</param>
        /// <returns></returns>
        public Task<IEnumerable<TagDTO>> GetAllWithPopularityAsync(bool? isConfirmed, int count = 50)
        {
            var query = "select tags.*, COUNT(tags.\"Id\") as \"Popularity\" " +
                "from dbo.\"Tags\" tags left outer join dbo.\"TagPosts\" tagposts on tagposts.\"Tag_Id\"=tags.\"Id\" " +
                "where \"Checked\"=true ";
            if(isConfirmed != null)
                query += "and \"IsConfirmed\"=@isConfirmed ";
            query += "group by tags.\"Id\" order by \"Popularity\" desc limit @count;";
            return Db.QueryAsync<TagDTO>(query, new { isConfirmed, count });
        }
        public async Task<IEnumerable<TagDTO>> GetAllWithPopularityAsync()
        {
            var query = "select tags.*, COUNT(tags.\"Id\") as \"Popularity\" "
                + "from dbo.\"Tags\" tags left outer join dbo.\"TagPosts\" tagposts on tagposts.\"Tag_Id\"=tags.\"Id\"" +
                " where \"Checked\"=true group by tags.\"Id\" order by \"Popularity\" desc;";
            return await Db.QueryAsync<TagDTO>(query);
        }
        public async Task<IEnumerable<Tag>> GetByNameAsync(string name)
        {
            var query = $"select * from dbo.\"Tags\" where \"Name\" like '%{name}%'";
            var tags = await Db.QueryAsync<Tag>(query);
            return tags;
        }
        /// <summary>
        /// Исходя из списка постов, получаем теги
        /// </summary>
        /// <param name="postsIds"></param>
        /// <returns></returns>
        public async Task<List<Tag>> GetByPostsAsync(IList<Guid> postsIds)
        {
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
            tags = tags.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            var query = "select * from dbo.\"Tags\" where \"Name\" = any(@Tags); "
                + "select * from dbo.\"TagPosts\" where \"Post_Id\"=@PostId;";
            var multi = await Db.QueryMultipleAsync(query, new { Tags = tags.Select(x => x.Name).ToList(), PostId = postId });

            var existingTags = await multi.ReadAsync<Tag>();
            var existingTagPosts = await multi.ReadAsync<TagPost>();
            multi.Dispose();

            //Теги для добавления в таблицу тегов
            var tagsToAdd = tags.Where(x => !existingTags.Any(y => y.Name == x.Name));
            foreach (var tag in tagsToAdd)
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
                .Select(x => (Guid)x.Tag_Id).ToList();
            //Теги для добавления к постам
            var tagPostsToAdd = tags.Where(x => !existingTagPosts.Any(y => y.Tag_Id == x.Id))
                .Select((x => new { TagId = x.Id, PostId = postId })).ToList();
            if (tagPostsToDel.Count > 0)
            {
                query = "delete from dbo.\"TagPosts\" where \"Tag_Id\" = any(@Tags) and \"Post_Id\"=@PostId;";
                await Db.ExecuteAsync(query, new { Tags = tagPostsToDel, PostId = postId });
            }
            query = "insert into dbo.\"TagPosts\"(\"Tag_Id\", \"Post_Id\") values(@TagId, @PostId);";
            await Db.ExecuteAsync(query, tagPostsToAdd);
        }
        public async Task AddWithProductAsync(List<Tag> tags, Guid productId)
        {
            tags = tags.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            var query = "select * from dbo.\"Tags\" where \"Name\"=any(@tags); "
                + "select * from dbo.\"ProductTags\" where \"ProductId\"=@productId;";
            var multi = await Db.QueryMultipleAsync(query, new { tags = tags.Select(x => x.Name).ToList(), productId });

            var existingTags = await multi.ReadAsync<Tag>();
            var existingTagPosts = await multi.ReadAsync();
            multi.Dispose();

            //Теги для добавления в таблицу тегов
            var tagsToAdd = tags.Where(x => !existingTags.Any(y => y.Name == x.Name));
            foreach (var tag in tagsToAdd)
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

            //Выбираем теги для удаления из товаров
            var productTagsToDel = existingTagPosts.Where(x => !existingTags.Any(y => y.Id == x.Tag_Id))
                .Select(x => (Guid)x.TagId).ToList();
            //Теги для добавления к товарам
            var productTagsToAdd = tags.Where(x => !existingTagPosts.Any(y => y.TagId == x.Id))
                .Select((x => new { TagId = x.Id, ProductId = productId })).ToList();
            if (productTagsToDel.Count > 0)
            {
                query = "delete from dbo.\"ProductTags\" where \"TagId\"=any(@tags) and \"ProductId\"=@productId;";
                await Db.ExecuteAsync(query, new { Tags = productTagsToDel, productId });
            }
            query = "insert into dbo.\"ProductTags\"(\"TagId\", \"ProductId\") values(@TagId, @ProductId);";
            await Db.ExecuteAsync(query, productTagsToAdd);
        }
        /// <summary>
        /// Получить теги, на которые подписан пользователь
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<IEnumerable<Tag>> GetForUserAsync(string userId)
        {
            var query = "select tags.* from dbo.\"UserTags\" usertags inner join dbo.\"Tags\" tags on tags.\"Id\"=usertags.\"TagId\" " +
                "where \"UserId\"=@userId;";
            return Db.QueryAsync<Tag>(query, new { userId });
        }
    }
}