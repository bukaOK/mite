using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.Enums;
using System.Text.RegularExpressions;
using System.Text;

namespace Mite.DAL.Repositories
{
    public sealed class PostsRepository : Repository<Post>
    {

        public PostsRepository(IDbConnection db) : base(db)
        {
        }
        public async override Task RemoveAsync(Guid id)
        {
            var query = "select top 1 UserId from dbo.Posts where Id=@id";
            var currentUserId = await Db.QueryFirstAsync<string>(query, new { id });
            var queryParams = new { Id = id, userId = currentUserId };
            //Удаляем пост
            //Удаляем с комментариев ссылку на пост, чтобы комменты не потеряли рейтинг(несправедливо жи)
            query = "update dbo.Comments set PostId=NULL where PostId=@id; delete from dbo.Ratings where PostId=@id; delete from dbo.Posts where Id=@id;";
            await Db.ExecuteAsync(query, queryParams);
            //Получаем новый рейтинг(нельзя все пихать в один запрос, т.к. новый рейтинг может быть null и выкинет исключение)
            query = "select SUM(Value) from dbo.Ratings where OwnerId=@userId";
            var newRating = (await Db.QueryFirstAsync<int?>(query, queryParams)) ?? 0;
            query = "update dbo.AspNetUsers set Rating=@newRating where Id=@userId";
            await Db.ExecuteAsync(query, new { Id = id, userId = currentUserId, newRating = newRating });
        }
        public override Task UpdateAsync(Post entity)
        {
            var query = "update dbo.Posts set Title=@Title, Content=@Content, IsImage=@IsImage, LastEdit=@LastEdit, IsPublished=@IsPublished, " +
                "Cover=@Cover, Description=@Description, Rating=@Rating, Views=@Views where Id=@Id";
            return Db.ExecuteAsync(query, entity);
        }
        public Task<IEnumerable<Post>> GetByUserAsync(string userId, bool isPublished)
        {
            var query = "select * from dbo.Posts where dbo.Posts.UserId=@UserId and IsPublished=@IsPublished";
            return Db.QueryAsync<Post>(query, new { UserId = userId, IsPublished = isPublished });
        }
        /// <summary>
        /// Возвращает пост с тегами
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public async Task<Post> GetWithTagsAsync(Guid id)
        {
            var query = "select * from dbo.Posts left outer join dbo.TagPosts on dbo.Posts.Id=dbo.TagPosts.Post_Id "
                + $"left outer join dbo.Tags on dbo.TagPosts.Tag_Id=dbo.Tags.Id where dbo.Posts.Id='{id}'";

            //Каждый возвращаемый объект представляет собой строку
            //поэтому используется такой костыль
            var tags = new List<Tag>();
            var post = (await Db.QueryAsync<Post, Tag, Post>(query, (postRow, tag) =>
            {
                if (tag != null)
                    tags.Add(tag);
                return postRow;
            })).FirstOrDefault();

            post.Tags = tags;
            return post;
        }
        /// <summary>
        /// Возвращает пост с тегами и комментариями
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public async Task<Post> GetWithTagsCommentsAsync(Guid id)
        {
            var query = "select * from dbo.Posts left outer join dbo.TagPosts on dbo.TagPosts.Post_Id=dbo.Posts.Id"
                + " left outer join dbo.Tags on dbo.TagPosts.Tag_Id=dbo.Tags.Id left outer join dbo.Comments"
                + $" on dbo.Comments.Post_Id=dbo.Posts.Id where dbo.Posts.Id='{id}'";

            return (await Db.QueryAsync<Post, Tag, Comment, Post>(query, (post, tag, comment) =>
            {
                post.Tags.Add(tag);
                post.Comments.Add(comment);
                return post;
            })).FirstOrDefault();
        }
        public Task<int> GetPublishedPostsCount(string userId)
        {
            return Db.QueryFirstAsync<int>("select COUNT(*) from dbo.Posts where UserId=@UserId and IsPublished=1", new { UserId = userId });
        }
        public Task AddView(Guid id)
        {
            return Db.ExecuteAsync("update dbo.Posts set Views=Views+1 where Id=@Id", new { Id = id });
        }
        public Task PublishPost(Guid id)
        {
            return Db.ExecuteAsync("update dbo.Posts set IsPublished=1 where Id=@Id", new { Id = id });
        }
        /// <summary>
        /// Получить посты по тегам
        /// </summary>
        /// <param name="tagIds">Список Id тегов</param>
        /// <param name="isPublished">Опубликованный ли пост</param>
        /// <param name="minDate">Дата, с которой начинается отбор постов</param>
        /// <param name="onlyFollowings">Выбрать только из тех на кого подписан</param>
        /// <param name="currentUserId">Id текущего пользователя</param>
        /// <param name="sortType">Как сортировать</param>
        /// <param name="offset">Сколько строк пропустить, прежде чем начать отбор</param>
        /// <param name="range">Сколько постов достать</param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> GetByTagsAsync(string[] tagsNames, bool isPublished, DateTime minDate, 
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range)
        {
            var tagNamesStr = new StringBuilder();
#if !DEBUG
            tagNamesStr.Append("CONTAINS(dbo.Tags.Name,"); 
#endif
            for (var i = 0; i < tagsNames.Length; i++)
            {
#if DEBUG
                tagNamesStr.AppendFormat("dbo.Tags.Name like N'%{0}%' ", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append("or ");
#else
                tagNamesStr.AppendFormat("'\"{0}*\"", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append(" or ");
                else
                    tagNamesStr.Append("')");
#endif
            }
            //Запрос для получения кол-ва совпадений тегов для каждого поста
            var tagsCountQuery = "select COUNT(Tag_Id) as TagsCount, Post_Id from dbo.Tags inner join dbo.TagPosts " +
                    $"on dbo.Tags.Id=dbo.TagPosts.Tag_Id where {tagNamesStr.ToString()} group by Post_Id";
            var tagsCountRes = await Db.QueryAsync(tagsCountQuery);
            //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега)
            var postsIds = tagsCountRes.Where(x => (int)x.TagsCount == tagsNames.Length)
                .Select<dynamic, Guid>(x => Guid.Parse(x.Post_Id.ToString()));

            var query = "select * from dbo.Posts inner join dbo.AspNetUsers on dbo.AspNetUsers.Id=dbo.Posts.UserId " +
                "where dbo.Posts.Id in @postsIds and IsPublished=@isPublished and LastEdit > @minDate ";

            IEnumerable<string> followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select FollowingUserId from dbo.Followers where UserId=@currentUserId";
                followings = await Db.QueryAsync<string>(followingsQuery, new { currentUserId });
                query += "and dbo.AspNetUsers.Id in @followings ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.Posts.Rating desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.Posts.LastEdit desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.Posts.LastEdit asc";
                    break;
            }
            query += $" offset {offset} rows fetch next {range} rows only";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { postsIds, isPublished, minDate, followings });
            return posts;
        }
        /// <summary>
        /// Получить посты по имени и списку тегов
        /// </summary>
        /// <param name="postName">Название работы</param>
        /// <param name="tagsNames">Список имен тегов</param>
        /// <param name="isPublished">Опубликованный ли пост</param>
        /// <param name="minDate">Дата, скоторой начинается отбор</param>
        /// <param name="onlyFollowings">Выбрать только из тех на кого подписан</param>
        /// <param name="currentUserId">Id текущего пользователя</param>
        /// <param name="sortType">Как сортировать</param>
        /// <param name="offset">Сколько строк пропустить, прежде чем начать отбор</param>
        /// <param name="range">Сколько нужно взять</param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> GetByPostNameAndTagsAsync(string postName, string[] tagsNames, bool isPublished, DateTime minDate,
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range)
        {
            var tagNamesStr = new StringBuilder();
#if !DEBUG
            tagNamesStr.Append("CONTAINS(dbo.Tags.Name, ");
#endif
            for (var i = 0; i < tagsNames.Length; i++)
            {
#if DEBUG
                tagNamesStr.AppendFormat("dbo.Tags.Name like N'%{0}%' ", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append("or ");
#else
                tagNamesStr.AppendFormat("'\"{0}*\"", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append(" or ");
                else
                    tagNamesStr.Append("')");
#endif
            }
            //Запрос для получения кол-ва совпадений тегов для каждого поста
            var tagsCountQuery = "select COUNT(Tag_Id) as TagsCount, Post_Id from dbo.Tags inner join dbo.TagPosts " +
                    $"on dbo.Tags.Id=dbo.TagPosts.Tag_Id where {tagNamesStr.ToString()} group by Post_Id";
            var tagsCountRes = await Db.QueryAsync(tagsCountQuery);
            //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега)
            var postsIds = tagsCountRes.Where(x => (int)x.TagsCount == tagsNames.Length)
                .Select<dynamic, Guid>(x => Guid.Parse(x.Post_Id.ToString()));

            var query = "select * from dbo.Posts inner join dbo.AspNetUsers on dbo.AspNetUsers.Id=dbo.Posts.UserId " +
                "where dbo.Posts.Id in @postsIds and IsPublished=@isPublished and LastEdit > @minDate ";
#if DEBUG
            query += $"and dbo.Posts.Title like N'%{postName}%'";
#else
            postName = "\"" + postName + "*\"";
            query += $"and CONTAINS(dbo.Posts.Title, @postName) ";
#endif
            IEnumerable<string> followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select FollowingUserId from dbo.Followers where UserId=@currentUserId ";
                followings = await Db.QueryAsync<string>(followingsQuery, new { currentUserId });
                query += "and dbo.AspNetUsers.Id in @followings ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.Posts.Rating desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.Posts.LastEdit desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.Posts.LastEdit asc";
                    break;
            }
            //Получаем по диапазону
            query += $" offset {offset} rows fetch next {range} rows only";
            
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { isPublished, minDate, postsIds, followings, postName });
            return posts;
        }
        public async Task<IEnumerable<Post>> GetByPostNameAsync(string postName, bool isPublished, DateTime minDate,
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range)
        {
            var query = "select * from dbo.Posts inner join dbo.AspNetUsers on dbo.Posts.UserId=dbo.AspNetUsers.Id " +
                "where dbo.Posts.IsPublished=@isPublished and LastEdit > @minDate and ";
#if DEBUG
            query += $"dbo.Posts.Title like N'%{postName}%'";
#else
            postName = "\"" + postName + "*\"";
            query += "CONTAINS(dbo.Posts.Title, @postName) ";
#endif
            IEnumerable<string> followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select FollowingUserId from dbo.Followers where UserId=@currentUserId ";
                followings = await Db.QueryAsync<string>(followingsQuery, new { currentUserId });
                query += "and dbo.AspNetUsers.Id in @followings ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.Posts.Rating desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.Posts.LastEdit desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.Posts.LastEdit asc";
                    break;
            }
            query += $" offset {offset} rows fetch next {range} rows only";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { isPublished, minDate, postName, followings });
            return posts;
        }
        public async Task<IEnumerable<Post>> GetByFilterAsync(bool isPublished, DateTime minDate,
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range)
        {
            var query = "select * from dbo.Posts inner join dbo.AspNetUsers on dbo.AspNetUsers.Id=dbo.Posts.UserId" +
                " where dbo.Posts.IsPublished=@isPublished and LastEdit > @minDate ";
            IEnumerable<string> followings;
            object queryParams = new { isPublished, minDate };
            if (onlyFollowings)
            {
                var followingsQuery = "select FollowingUserId from dbo.Followers where UserId=@currentUserId ";
                followings = await Db.QueryAsync<string>(followingsQuery, new { currentUserId });
                query += "and dbo.AspNetUsers.Id in @followings ";
                queryParams = new { isPublished, minDate, followings };
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.Posts.Rating desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.Posts.LastEdit desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.Posts.LastEdit asc";
                    break;
            }
            query += $" offset {offset} rows fetch next {range} rows only";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, queryParams);
            return posts;
        }
        public Task<IEnumerable<Post>> GetGalleryByUserAsync(string userId)
        {
            var query = "select Id, Content from dbo.Posts where UserId=@userId and IsPublished=1 and IsImage=1";
            return Db.QueryAsync<Post>(query, new { userId });
        }
    }
}