using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;

namespace Mite.DAL.Repositories
{
    public sealed class TagsRepository : Repository<Tag>
    {
        public TagsRepository(IDbConnection db) : base(db)
        {
        }
        public Task<IEnumerable<Tag>> GetAllAsync(bool isConfirmed)
        {
            return Db.QueryAsync<Tag>("select * from dbo.Tags where IsConfirmed=@isConfirmed", new { isConfirmed });
        }
        public Task<IEnumerable<Tag>> GetByNameAsync(string name)
        {
            
            return Db.QueryAsync<Tag>("select * from dbo.Tags where Name like @name", name);
        }
        /// <summary>
        /// Меняем один тег на другой(если например имя старого тега неправильно записано)
        /// </summary>
        /// <returns></returns>
        public Task ChangeAsync(Guid oldTagId, Guid newTagId)
        {
            var query = "update dbo.TagPosts set Tag_Id = @newTagId where Tag_Id = @oldTagId; "
                + "delete from dbo.Tags where Id=@oldTagId";
            return Db.ExecuteAsync(query, new { oldTagId, newTagId });
        }
        public async override Task AddAsync(Tag entity)
        {
            var sameTags = await Db.QueryAsync<Tag>("select top 1 * from dbo.Tags where Name=@Name", new { Name = entity.Name });
            if (sameTags.Count() > 0)
                return;
            var query = "insert into dbo.Tags (Id, Name, IsConfirmed) values (@Id, @Name, @IsConfirmed)";
            await Db.ExecuteAsync(query, entity);
        }
        public async Task AddWithPostAsync(List<Tag> tags, Guid postId)
        {
            var query = "select * from dbo.Tags where Name in @Tags; "
                + "select * from dbo.TagPosts where Post_Id=@PostId";
            var multi = await Db.QueryMultipleAsync(query, new { Tags = tags.Select(x => x.Name), PostId = postId });

            var existingTags = await multi.ReadAsync<Tag>();
            var existingTagPosts = await multi.ReadAsync();
            multi.Dispose();

            //Теги для добавления в таблицу тегов
            var tagsToAdd = tags.Where(x => !existingTags.Any(y => y.Name == x.Name));
            foreach(var tag in tagsToAdd)
            {
                tag.Id = Guid.NewGuid();
            }
            query = "insert into dbo.Tags (Id, Name, IsConfirmed) values (@Id, @Name, @IsConfirmed); ";
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
            query = "delete from dbo.TagPosts where Tag_Id in @Tags and Post_Id=@PostId";
            await Db.ExecuteAsync(query, new { Tags = tagPostsToDel, PostId = postId });
            query = "insert into dbo.TagPosts(Tag_Id, Post_Id) values(@TagId, @PostId)";
            await Db.ExecuteAsync(query, tagPostsToAdd);
        }
    }
}