using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.CodeData.Enums;
using System.Text;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public sealed class PostsRepository : Repository<Post>
    {

        public PostsRepository(AppDbContext db) : base(db)
        {
        }
        public async override Task RemoveAsync(Guid id)
        {
            var query = "select \"UserId\" from dbo.\"Posts\" where \"Id\"=@id;";
            var currentUserId = await Db.QueryFirstAsync<string>(query, new { id });
            var queryParams = new { Id = id, userId = currentUserId };
            //Удаляем пост
            //Удаляем с комментариев ссылку на пост, чтобы комменты не потеряли рейтинг(несправедливо жи)
            query = "update dbo.\"Comments\" set \"PostId\"=NULL where \"PostId\"=@id; delete from dbo.\"Ratings\" " + 
                "where \"PostId\"=@id; delete from dbo.\"Posts\" where \"Id\"=@id;";
            await Db.ExecuteAsync(query, queryParams);
            //Получаем новый рейтинг(нельзя все пихать в один запрос, т.к. новый рейтинг может быть null и выкинет исключение)
            query = "select SUM(\"Value\") from dbo.\"Ratings\" where \"OwnerId\"=@userId;";
            var newRating = (await Db.QueryFirstAsync<int?>(query, queryParams)) ?? 0;
            query = "update dbo.\"Users\" set \"Rating\"=@newRating where \"Id\"=@userId;";
            await Db.ExecuteAsync(query, new { Id = id, userId = currentUserId, newRating = newRating });
        }
        /// <summary>
        /// Получить посты по пользователю
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="isPublished">Опубликованные или нет</param>
        /// <param name="blocked">Заблокированные или нет</param>
        /// <param name="offset">Сколько строк пропустить</param>
        /// <param name="range">Сколько достать</param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> GetByUserAsync(string userId, bool isPublished, bool blocked, SortFilter sort)
        {
            var query = "select * from dbo.\"Posts\" where \"UserId\"=@userId and \"Blocked\"=@blocked ";
            if (isPublished)
            {
                query += "and \"PublishDate\" is not null ";
            }
            else
            {
                query += "and \"PublishDate\" is null ";
            }
            switch (sort)
            {
                case SortFilter.Popular:
                    query += "order by \"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by \"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by \"PublishDate\" asc";
                    break;
                default:
                    throw new ArgumentException("Неизвестный тип сортировки");
            }
            query += ";";
            var posts = await Db.QueryAsync<Post>(query, new { blocked, userId });
            return posts;
        }
        /// <summary>
        /// Возвращает пост с тегами
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public Task<Post> GetWithTagsAsync(Guid id)
        {
            return DbContext.Posts.Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == id);
        }
        /// <summary>
        /// Возвращает пост с тегами и комментариями
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public async Task<Post> GetWithTagsCommentsAsync(Guid id)
        {
            var query = "select * from dbo.\"Posts\" left outer join dbo.\"TagPosts\" on dbo.\"TagPosts\".\"Post_Id\"=dbo.\"Posts\".\"Id\""
                + " left outer join dbo.\"Tags\" on dbo.\"TagPosts\".\"Tag_Id\"=dbo.\"Tags\".\"Id\" left outer join dbo.\"Comments\""
                + $" on dbo.\"Comments\".\"Post_Id\"=dbo.\"Posts\".\"Id\" where dbo.\"Posts\".\"Id\"=@id;";

            return (await Db.QueryAsync<Post, Tag, Comment, Post>(query, (post, tag, comment) =>
            {
                post.Tags.Add(tag);
                post.Comments.Add(comment);
                return post;
            }, new { id })).FirstOrDefault();
        }
        public Task<int> GetPublishedPostsCount(string userId)
        {
            return Table.CountAsync(x => x.UserId == userId && x.PublishDate != null && !x.Blocked);
        }
        public async Task AddView(Guid id)
        {
            var post = await Table.FirstAsync(x => x.Id == id);
            post.Views++;
            DbContext.Entry(post).Property(x => x.Views).IsModified = true;
            await SaveAsync();
        }
        public async Task PublishPost(Guid id, DateTime publishDate)
        {
            var post = await Table.FirstAsync(x => x.Id == id);
            post.PublishDate = publishDate;
            DbContext.Entry(post).Property(x => x.PublishDate).IsModified = true;
            await SaveAsync();
        }
        /// <summary>
        /// Получить посты по тегам
        /// </summary>
        /// <param name="tagIds">Список Id тегов</param>
        /// <param name="minDate">Дата, с которой начинается отбор постов</param>
        /// <param name="onlyFollowings">Выбрать только из тех на кого подписан</param>
        /// <param name="currentUserId">Id текущего пользователя</param>
        /// <param name="sortType">Как сортировать</param>
        /// <param name="offset">Сколько строк пропустить, прежде чем начать отбор</param>
        /// <param name="range">Сколько постов достать</param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> GetByTagsAsync(string[] tagsNames, DateTime minDate, 
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range, DateTime maxDate)
        {
            var tagNamesStr = new StringBuilder();
            for (var i = 0; i < tagsNames.Length; i++)
            {
                tagNamesStr.AppendFormat("dbo.\"Tags\".\"Name\" like N'%{0}%' ", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append("or ");
            }
            //Запрос для получения кол-ва совпадений тегов для каждого поста
            var tagsCountQuery = "select COUNT(\"Tag_Id\") as \"TagsCount\", \"Post_Id\" from dbo.\"Tags\" inner join dbo.\"TagPosts\" " +
                    $"on dbo.\"Tags\".\"Id\"=dbo.\"TagPosts\".\"Tag_Id\" where {tagNamesStr.ToString()} group by \"Post_Id\";";
            var tagsCountRes = await Db.QueryAsync(tagsCountQuery);
            //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега)
            var postsIds = tagsCountRes.Where(x => (int)x.TagsCount >= tagsNames.Length)
                .Select<dynamic, Guid>(x => Guid.Parse(x.Post_Id.ToString())).ToList();

            var query = "select * from dbo.\"Posts\" inner join dbo.\"Users\" on dbo.\"Users\".\"Id\"=dbo.\"Posts\".\"UserId\" " +
                "where dbo.\"Posts\".\"Id\" = any(@postsIds) and \"PublishDate\" is not null and dbo.\"Posts\".\"Blocked\"=false and \"PublishDate\" > @minDate " +
                "and \"PublishDate\" < @maxDate ";

            var followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select \"FollowingUserId\" from dbo.\"Followers\" where \"UserId\"=@currentUserId;";
                followings = (await Db.QueryAsync<string>(followingsQuery, new { currentUserId })).ToList();
                query += "and dbo.\"Users\".\"Id\" = any(@followings) ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.\"Posts\".\"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.\"Posts\".\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.\"Posts\".\"PublishDate\" asc";
                    break;
            }
            query += $" offset {offset} limit {range};";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { postsIds, minDate, followings, maxDate });
            return posts;
        }
        /// <summary>
        /// Получить посты по имени и списку тегов
        /// </summary>
        /// <param name="postName">Название работы</param>
        /// <param name="tagsNames">Список имен тегов</param>
        /// <param name="minDate">Дата, скоторой начинается отбор</param>
        /// <param name="onlyFollowings">Выбрать только из тех на кого подписан</param>
        /// <param name="currentUserId">Id текущего пользователя</param>
        /// <param name="sortType">Как сортировать</param>
        /// <param name="offset">Сколько строк пропустить, прежде чем начать отбор</param>
        /// <param name="range">Сколько нужно взять</param>
        /// <param name="maxDate">Дата, после которой не берутся посты(чтобы не раздваивались)</param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> GetByPostNameAndTagsAsync(string postName, string[] tagsNames, DateTime minDate,
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range, DateTime maxDate)
        {
            var tagNamesStr = new StringBuilder();
            for (var i = 0; i < tagsNames.Length; i++)
            {
                tagNamesStr.AppendFormat("dbo.\"Tags\".\"Name\" like N'%{0}%' ", tagsNames[i]);
                if (i < tagsNames.Length - 1)
                    tagNamesStr.Append("or ");
            }
            //Запрос для получения кол-ва совпадений тегов для каждого поста
            var tagsCountQuery = "select COUNT(\"Tag_Id\") as \"TagsCount\", \"Post_Id\" from dbo.\"Tags\" inner join dbo.\"TagPosts\" " +
                    $"on dbo.\"Tags\".\"Id\"=dbo.\"TagPosts\".\"Tag_Id\" where {tagNamesStr.ToString()} group by \"Post_Id\"";
            var tagsCountRes = await Db.QueryAsync(tagsCountQuery);
            //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега или больше)
            var postsIds = tagsCountRes.Where(x => (int)x.TagsCount >= tagsNames.Length)
                .Select(x => (Guid)x.Post_Id).ToList();

            var query = "select * from dbo.\"Posts\" inner join dbo.\"Users\" on dbo.\"Users\".\"Id\"=dbo.\"Posts\".\"UserId\" " +
                "where dbo.\"Posts\".\"Id\" = any(@postsIds) and \"PublishDate\" is not null " +
                "and dbo.\"Posts\".\"Blocked\"=false and \"PublishDate\" > @minDate and \"PublishDate\" < @maxDate ";

            query += "and (setweight(to_tsvector('mite_ru', dbo.\"Posts\".\"Title\"), 'A') || " +
                "setweight(to_tsvector('mite_ru', coalesce(dbo.\"Posts\".\"Description\", '')), 'B')) @@ plainto_tsquery(@postName)";

            List<string> followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select \"FollowingUserId\" from dbo.\"Followers\" where \"UserId\"=@currentUserId ";
                followings = (await Db.QueryAsync<string>(followingsQuery, new { currentUserId })).ToList();
                query += "and dbo.\"Users\".\"Id\" = any(@followings) ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.\"Posts\".\"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.\"Posts\".\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.\"Posts\".\"PublishDate\" asc";
                    break;
            }
            //Получаем по диапазону
            query += $" limit {range} offset {offset};";
            
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { minDate, postsIds, followings, postName, maxDate });
            return posts;
        }
        public async Task<IEnumerable<Post>> GetByPostNameAsync(string postName, DateTime minDate,
            bool onlyFollowings, string currentUserId, SortFilter sortType, int offset, int range, DateTime maxDate)
        {
            var query = "select * from dbo.\"Posts\" inner join dbo.\"Users\" on dbo.\"Posts\".\"UserId\"=dbo.\"Users\".\"Id\" " +
                "where dbo.\"Posts\".\"PublishDate\" is not null and dbo.\"Posts\".\"Blocked\"=false and \"PublishDate\" > @minDate " +
                "and \"PublishDate\" < @maxDate ";
            query += "and (setweight(to_tsvector('mite_ru', dbo.\"Posts\".\"Title\"), 'A') || " +
                "setweight(to_tsvector('mite_ru', coalesce(dbo.\"Posts\".\"Description\", '')), 'B')) @@ plainto_tsquery(@postName)";

            var followings = new List<string>();
            if (onlyFollowings)
            {
                var followingsQuery = "select \"FollowingUserId\" from dbo.\"Followers\" where \"UserId\"=@currentUserId;";
                followings = (await Db.QueryAsync<string>(followingsQuery, new { currentUserId })).ToList();
                query += "and dbo.\"Users\".\"Id\" = any(@followings) ";
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.\"Posts\".\"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.\"Posts\".\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.\"Posts\".\"PublishDate\" asc";
                    break;
            }
            query += $" limit {range} offset {offset};";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, new { minDate, postName, followings, maxDate });
            return posts;
        }
        public async Task<IEnumerable<Post>> GetByFilterAsync(DateTime minDate, bool onlyFollowings, 
            string currentUserId, SortFilter sortType, int offset, int range, DateTime maxDate)
        {
            var query = "select * from dbo.\"Posts\" inner join dbo.\"Users\" on dbo.\"Users\".\"Id\"=dbo.\"Posts\".\"UserId\"" +
                " where \"PublishDate\" is not null and dbo.\"Posts\".\"Blocked\"=false and \"PublishDate\" > @minDate " +
                "and \"PublishDate\" < @maxDate ";
            var followings = new List<string>();
            object queryParams = new { minDate, maxDate };
            if (onlyFollowings)
            {
                var followingsQuery = "select \"FollowingUserId\" from dbo.\"Followers\" where \"UserId\"=@currentUserId;";
                followings = (await Db.QueryAsync<string>(followingsQuery, new { currentUserId })).ToList();
                query += "and dbo.\"Users\".\"Id\" = any(@followings) ";
                queryParams = new { minDate, followings, maxDate };
            }
            switch (sortType)
            {
                case SortFilter.Popular:
                    query += "order by dbo.\"Posts\".\"Rating\" desc, dbo.\"Posts\".\"PublishDate\" desc";
                    break;
                case SortFilter.New:
                    query += "order by dbo.\"Posts\".\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by dbo.\"Posts\".\"PublishDate\" asc";
                    break;
            }
            query += $" limit {range} offset {offset};";
            var posts = await Db.QueryAsync<Post, User, Post>(query, (post, user) =>
            {
                post.User = user;
                return post;
            }, queryParams);
            return posts;
        }
        public async Task<IEnumerable<Post>> GetGalleryByUserAsync(string userId)
        {
            return await Table.Where(x => x.UserId == userId && x.PublishDate != null && !x.Blocked && x.IsImage).ToListAsync();
        }
    }
}