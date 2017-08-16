using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using Mite.CodeData.Enums;
using System;
using System.Linq;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class CommentsRepository : Repository<Comment>
    {
        public CommentsRepository(AppDbContext db) : base(db)
        {
        }

        public Task<int> GetPostCommentsCountAsync(Guid postId)
        {
            return Table.CountAsync(x => x.PostId == postId);
        }
        public override Task AddAsync(Comment entity)
        {
            if(entity.ParentCommentId != null)
            {
                entity.ParentComment = Table.Find(entity.ParentCommentId);
            }
            Table.Add(entity);
            return SaveAsync();
        }
        public async Task<IDictionary<Guid, int>> GetPostsCommentsCountAsync(IEnumerable<Guid> postIds)
        {
            var result = await Table.Where(x => postIds.Any(y => y == x.PostId)).GroupBy(x => x.PostId)
                .ToDictionaryAsync(x => (Guid)x.Key, x => x.Count());
            return result;
        }
        /// <summary>
        /// Возвращает отсортированные комментарии к посту
        /// </summary>
        /// <param name="postId">Id поста</param>
        /// <param name="rowsCount">кол-во записей</param>
        /// <param name="sortParam">тип сортировки</param>
        /// <returns></returns>
        public async Task<IEnumerable<Comment>> GetListByPostAsync(Guid postId)
        {
            var comments = await Table.Include(x => x.User).Where(x => x.PostId == postId).ToListAsync();
            return comments;
        }
        public override async Task RemoveAsync(Guid id)
        {
            var query = "select \"UserId\" from dbo.\"Comments\" where \"Id\"=@Id;";
            var currentUserId = await Db.QueryFirstAsync<string>(query, new { Id = id });
            query = "update dbo.\"Comments\" set \"ParentCommentId\"=null where \"ParentCommentId\"=@Id;" +
                "delete from dbo.\"Ratings\" where \"CommentId\"=@Id;delete from dbo.\"Comments\" where \"Id\"=@Id;";
            await Db.ExecuteAsync(query, new { Id = id, UserId = currentUserId });
            //Получаем новый рейтинг(нельзя все пихать в один запрос, т.к. новый рейтинг может быть null и выкинет исключение)
            query = "select SUM(\"Value\") from dbo.\"Ratings\" where \"OwnerId\"=@UserId;";
            var newRating = (await Db.QueryFirstAsync<int?>(query, new { Id = id, UserId = currentUserId })) ?? 0;

            query = "update dbo.\"Users\" set \"Rating\"=@newRating where \"Id\"=@UserId;";
            await Db.ExecuteAsync(query, new { Id = id, UserId = currentUserId, newRating = newRating });
        }
        /// <summary>
        /// Возвращает комментарий с родительским комментом и пользователем
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Comment> GetFullAsync(Guid id)
        {
            var comment = await Table.Include(x => x.User).Include(x => x.ParentComment.User).FirstAsync(x => x.Id == id);
            return comment;
        }
    }
}