using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Collections.Generic;
using Mite.Enums;
using System;
using System.Linq;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class CommentsRepository : Repository<Comment>
    {
        public CommentsRepository(AppDbContext db) : base(db)
        {
        }

        public Task<int> GetPostCommentsCountAsync(Guid postId)
        {
            return Db.QueryFirstAsync<int>("select COUNT(Id) from dbo.Comments where PostId=@PostId", new { PostId = postId });
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
            var result = await Db.QueryAsync("select COUNT(Id) as CommentsCount, PostId from dbo.Comments where PostId in @postIds group by PostId", new { postIds });
            var dict = new Dictionary<Guid, int>();
            foreach(var item in result)
            {
                dict.Add((Guid)item.PostId, (int)item.CommentsCount);
            }
            return dict;
        }
        /// <summary>
        /// Возвращает отсортированные комментарии к посту
        /// </summary>
        /// <param name="postId">Id поста</param>
        /// <param name="rowsCount">кол-во записей</param>
        /// <param name="sortParam">тип сортировки</param>
        /// <param name="descOrAsc">по убыванию или возрастанию</param>
        /// <returns></returns>
        public Task<IEnumerable<Comment>> GetListByPostAsync(string postId)
        {
            var query = "select * from dbo.Comments left outer join dbo.AspNetUsers on"
                + " dbo.Comments.UserId=dbo.AspNetUsers.Id where PostId=@PostId";
            return Db.QueryAsync<Comment, User, Comment>(query, (comment, user) =>
            {
                comment.User = user;
                return comment;
            }, new { PostId = postId });
        }
        public override async Task RemoveAsync(Guid id)
        {
            var query = "select top 1 UserId from dbo.Comments where Id=@Id";
            var currentUserId = await Db.QueryFirstAsync<string>(query, new { Id = id });

            query = "update dbo.Comments set ParentCommentId=null where ParentCommentId=@Id;" +
                "delete from dbo.Ratings where CommentId=@Id;delete from dbo.Comments where Id=@Id;";
            await Db.ExecuteAsync(query, new { Id = id, UserId = currentUserId });
            //Получаем новый рейтинг(нельзя все пихать в один запрос, т.к. новый рейтинг может быть null и выкинет исключение)
            query = "select SUM(Value) from dbo.Ratings where OwnerId=@UserId";
            var newRating = (await Db.QueryFirstAsync<int?>(query, new { Id = id, UserId = currentUserId })) ?? 0;

            query = "update dbo.AspNetUsers set Rating=@newRating where Id=@UserId;";
            await Db.ExecuteAsync(query, new { Id = id, UserId = currentUserId, newRating = newRating });
        }
        /// <summary>
        /// Возвращает комментарий с родительским комментом и пользователем
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Comment> GetFullAsync(Guid id)
        {
            var query = "select * from dbo.Comments, dbo.AspNetUsers where dbo.Comments.Id=@id and dbo.AspNetUsers.Id=dbo.Comments.UserId";
            var comment = (await Db.QueryAsync<Comment, User, Comment>(query, (com, user) =>
             {
                 com.User = user;
                 return com;
             }, new { id })).First();
            
            if(comment.ParentCommentId != null)
            {
                query = "select * from dbo.Comments, dbo.AspNetUsers where dbo.Comments.Id=@parentId and dbo.AspNetUsers.Id=dbo.Comments.UserId";
                comment.ParentComment = (await Db.QueryAsync<Comment, User, Comment>(query, (com, user) =>
                {
                    com.User = user;
                    return com;
                }, new { parentId = comment.ParentCommentId })).First();
            }
            return comment;
        }
    }
}