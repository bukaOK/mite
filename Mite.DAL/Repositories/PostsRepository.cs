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
using Mite.DAL.Filters;
using Mite.DAL.DTO;

namespace Mite.DAL.Repositories
{
    public sealed class PostsRepository : Repository<Post>
    {

        public PostsRepository(AppDbContext db) : base(db)
        {
        }
        public Task<Post> GetWithCollectionsAsync(Guid id)
        {
            return Table.AsNoTracking().Include(x => x.Collection).Include(x => x.ComicsItems).FirstOrDefaultAsync(x => x.Id == id);
        }
        public override async Task UpdateAsync(Post entity)
        {
            var existingPost = await Table.Include(x => x.Collection).Include(x => x.ComicsItems)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (entity.Collection.Any())
            {
                var itemsToUpdate = existingPost.Collection.Where(x => entity.Collection.Any(y => y.Id == x.Id));
                var itemsToAdd = entity.Collection.Where(x => !existingPost.Collection.Any(y => y.Id == x.Id));
                var itemsToDel = existingPost.Collection.Except(itemsToUpdate).ToList();

                foreach (var item in itemsToUpdate)
                {
                    DbContext.Entry(item).CurrentValues.SetValues(entity.Collection.First(x => x.Id == item.Id));
                }
                for(var i = 0; i < itemsToDel.Count; i++)
                {
                    DbContext.Entry(itemsToDel[i]).State = EntityState.Deleted;
                }
                foreach (var item in itemsToAdd)
                {
                    existingPost.Collection.Add(item);
                    DbContext.Entry(item).State = EntityState.Added;
                }
            }
            else if (entity.ComicsItems.Any())
            {
                var itemsToUpdate = existingPost.ComicsItems.Where(x => entity.ComicsItems.Any(y => y.Id == x.Id));
                var itemsToAdd = entity.ComicsItems.Where(x => !existingPost.ComicsItems.Any(y => y.Id == x.Id));
                var itemsToDel = existingPost.ComicsItems.Except(itemsToUpdate).ToList();

                foreach (var item in itemsToUpdate)
                {
                    DbContext.Entry(item).CurrentValues.SetValues(entity.ComicsItems.First(x => x.Id == item.Id));
                }
                for(var i = 0; i < itemsToDel.Count; i++)
                {
                    DbContext.Entry(itemsToDel[i]).State = EntityState.Deleted;
                }
                foreach (var item in itemsToAdd)
                {
                    existingPost.ComicsItems.Add(item);
                    DbContext.Entry(item).State = EntityState.Added;
                }
            }
            DbContext.Entry(existingPost).CurrentValues.SetValues(entity);
            await SaveAsync();
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
            await Db.ExecuteAsync(query, new { Id = id, userId = currentUserId, newRating });
        }
        /// <summary>
        /// Получить посты по пользователю
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="postType">Тип поста</param>
        /// <returns></returns>
        public async Task<IEnumerable<PostDTO>> GetByUserAsync(string userId, PostTypes postType, SortFilter sort)
        {
            var query = "select posts.*, (select count(*) from dbo.\"Comments\" as comments where comments.\"PostId\"=posts.\"Id\") " +
                "as \"CommentsCount\", tags.* from dbo.\"Posts\" as posts " +
                "left outer join dbo.\"TagPosts\" as tag_posts on tag_posts.\"Post_Id\"=posts.\"Id\" " +
                "left outer join dbo.\"Tags\" as tags on tags.\"Id\"=tag_posts.\"Tag_Id\" ";
            if (postType == PostTypes.Favorite)
                query += "right outer join dbo.\"FavoritePosts\" as favorites on favorites.\"PostId\"=posts.\"Id\" where favorites.\"UserId\"=@userId ";
            else
                query += "where posts.\"UserId\"=@userId and posts.\"Type\"=@postType ";
            switch (sort)
            {
                case SortFilter.Popular:
                    query += "order by posts.\"Rating\" desc";
                    break;
                case SortFilter.New:
                    query += "order by posts.\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "order by posts.\"PublishDate\" asc";
                    break;
                default:
                    throw new ArgumentException("Неизвестный тип сортировки");
            }
            query += ";";
            var posts = new List<PostDTO>();
            await Db.QueryAsync<PostDTO, Tag, PostDTO>(query, (post, tag) =>
            {
                var dto = posts.FirstOrDefault(x => x.Id == post.Id);
                if (dto == null)
                {
                    post.Tags = new List<Tag>();
                    if (tag != null)
                        post.Tags.Add(tag);
                    posts.Add(post);
                }
                else if (tag != null)
                    dto.Tags.Add(tag);
                return post;
            }, new { postType, userId });
            return posts;
        }
        /// <summary>
        /// Возвращает пост с тегами
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public Task<Post> GetWithTagsAsync(Guid id)
        {
            return DbContext.Posts.Include(x => x.Tags).Include(x => x.Collection).Include(x => x.ComicsItems).FirstOrDefaultAsync(x => x.Id == id);
        }
        /// <summary>
        /// Возвращает пост с тегами и комментариями
        /// </summary>
        /// <param name="id">Id поста</param>
        /// <returns></returns>
        public async Task<Post> GetWithTagsCommentsAsync(Guid id)
        {
            var query = "select * from dbo.\"Posts\" left outer join dbo.\"TagPosts\" on dbo.\"TagPosts\".\"Post_Id\"=dbo.\"Posts\".\"Id\""
                + " full outer join dbo.\"Tags\" on dbo.\"TagPosts\".\"Tag_Id\"=dbo.\"Tags\".\"Id\" full outer join dbo.\"Comments\""
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
            return Table.CountAsync(x => x.UserId == userId && x.PublishDate != null && x.Type != PostTypes.Blocked);
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
            post.Type = PostTypes.Published;
            DbContext.Entry(post).Property(x => x.PublishDate).IsModified = true;
            await SaveAsync();
        }
        public async Task<IEnumerable<PostDTO>> GetByFilterAsync(PostTopFilter filter)
        {
            var query = "select posts.\"Id\" from dbo.\"Posts\" as posts ";
            if (filter.OnlyFollowings)
                query += "inner join dbo.\"Users\" as users on users.\"Id\"=posts.\"UserId\" ";
            query += "where posts.\"Type\"=@PostType and \"PublishDate\" is not null and \"PublishDate\" > @MinDate and \"PublishDate\" < @MaxDate ";
            if (!string.IsNullOrEmpty(filter.PostName))
            {
                query += "and (setweight(to_tsvector('mite_ru', posts.\"Title\"), 'A') || " +
                    "setweight(to_tsvector('mite_ru', coalesce(posts.\"Description\", '')), 'B')) @@ plainto_tsquery(@PostName) ";
            }
            if(filter.Tags != null && filter.Tags.Length > 0)
            {
                var tagNamesStr = new StringBuilder();
                for (var i = 0; i < filter.Tags.Length; i++)
                {
                    tagNamesStr.AppendFormat("tags.\"Name\"='{0}' ", filter.Tags[i]);
                    if (i < filter.Tags.Length - 1)
                        tagNamesStr.Append("or ");
                }
                //Запрос для получения кол-ва совпадений тегов для каждого поста
                var tagsCountQuery = "select COUNT(\"Tag_Id\") as \"TagsCount\", \"Post_Id\" from dbo.\"Tags\" as tags " +
                        "inner join dbo.\"TagPosts\" as tag_posts on tags.\"Id\"=tag_posts.\"Tag_Id\" " +
                        $"where {tagNamesStr.ToString()} group by \"Post_Id\";";

                var tagsCountRes = await Db.QueryAsync(tagsCountQuery);
                //Чтобы у поста было точное кол-во совпадений с тегами(т.е. написали в запросе 2 тега - должно совпасть 2 тега или больше)
                filter.TagPostsIds = tagsCountRes.Where(x => (int)x.TagsCount >= filter.Tags.Length)
                    .Select(x => (Guid)x.Post_Id).ToList();

                query += "and posts.\"Id\"=any(@TagPostsIds) ";
            }
            if (filter.OnlyFollowings)
            {
                var folQuery = "select \"FollowingUserId\" from dbo.\"Followers\" where \"UserId\"=@CurrentUserId;";
                filter.Followings = (await Db.QueryAsync<string>(folQuery, filter)).ToList();
                query += "and users.\"Id\" = any(@Followings) ";
            }
            var sortQuery = "order by posts.\"PublishDate\" desc";
            switch (filter.SortType)
            {
                case SortFilter.Popular:
                    sortQuery = "order by posts.\"Rating\" desc, posts.\"PublishDate\" desc";
                    break;
                case SortFilter.New:
                    sortQuery = "order by posts.\"PublishDate\" desc";
                    break;
                case SortFilter.Old:
                    sortQuery = "order by posts.\"PublishDate\" asc";
                    break;
            }
            query += $"{sortQuery} limit {filter.Range} offset {filter.Offset};";
            filter.PostIds = (await Db.QueryAsync<Guid>(query, filter)).ToList();

            query = "select posts.*, (select count(*) from dbo.\"Comments\" as comments where comments.\"PostId\"=posts.\"Id\") " +
                "as \"CommentsCount\", users.*, tags.* from dbo.\"Posts\" as posts " +
                "inner join dbo.\"Users\" as users on posts.\"UserId\"=users.\"Id\" " +
                "left outer join dbo.\"TagPosts\" as tag_posts on tag_posts.\"Post_Id\"=posts.\"Id\" " +
                $"left outer join dbo.\"Tags\" as tags on tags.\"Id\"=tag_posts.\"Tag_Id\" where posts.\"Id\"=any(@PostIds) {sortQuery};";

            var posts = new List<PostDTO>();
            await Db.QueryAsync<PostDTO, User, Tag, PostDTO>(query, (post, user, tag) =>
            {
                var dto = posts.FirstOrDefault(x => x.Id == post.Id);
                if (dto == null)
                {
                    post.User = user;
                    post.Tags = new List<Tag>();
                    if (tag != null)
                        post.Tags.Add(tag);
                    posts.Add(post);
                }
                else if (tag != null)
                    dto.Tags.Add(tag);
                return post;
            }, filter);
            return posts;
        }
        public Task<List<Guid>> GetAllIdsAsync()
        {
            return Table.Select(x => x.Id).ToListAsync();
        }
        public async Task<IEnumerable<Post>> GetGalleryByUserAsync(string userId)
        {
            return await Table.AsNoTracking().Where(x => x.UserId == userId && x.Type == PostTypes.Published && x.ContentType != PostContentTypes.Document)
                .ToListAsync();
        }
    }
}