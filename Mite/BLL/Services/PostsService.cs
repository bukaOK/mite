using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Helpers;
using Mite.Models;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Enums;
using System.Collections.Generic;
using System.Web.Hosting;
using NLog;
using Mite.DAL.Repositories;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.DAL.Filters;

namespace Mite.BLL.Services
{
    public interface IPostsService : IDataService
    {
        Task<PostModel> GetPostAsync(Guid postId);
        Task<DataServiceResult> AddPostAsync(PostModel postModel, string userId);
        Task<DataServiceResult> DeletePostAsync(Guid postId);
        Task<DataServiceResult> UpdatePostAsync(PostModel postModel);
        Task<PostModel> GetWithTagsAsync(Guid postId);
        /// <summary>
        /// Возвращает вместе с тегами и владельцем поста
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<PostModel> GetWithTagsUserAsync(Guid postId);
        /// <summary>
        /// Получить работы по пользователю
        /// </summary>
        /// <param name="userName">Id автора</param>
        /// <param name="currentUserId">Id текущего пользователя(nullable)</param>
        /// <param name="sort">Сортировка</param>
        /// <param name="type">Тип работы</param>
        /// <returns></returns>
        Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userName, string currentUserId, SortFilter sort, PostTypes type);
        Task<GalleryModel> GetGalleryByUserAsync(string userId);
        /// <summary>
        /// Добавляем к посту один просмотр
        /// </summary>
        /// <param name="postId">Id поста</param>
        /// <returns></returns>
        Task AddViews(Guid postId);
        Task<DataServiceResult> PublishPost(Guid postId);
        Task<IEnumerable<TopPostModel>> GetTopAsync(PostTopFilterModel filter, string currentUserId);
    }
    public class PostsService : DataService, IPostsService
    {
        private readonly AppUserManager _userManager;
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsService(IUnitOfWork unitOfWork, AppUserManager userManager, ILogger logger) : base(unitOfWork, logger)
        {
            _userManager = userManager;
        }

        public async Task<PostModel> GetPostAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var post = await repo.GetAsync(postId);
            var postModel = Mapper.Map<PostModel>(post);

            //postModel.ContentType = FilesHelper.IsPath(postModel.Content);
            postModel.User = new UserShortModel
            {
                Id = post.UserId
            };
            return postModel;
        }
        public async Task<DataServiceResult> AddPostAsync(PostModel postModel, string userId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var tagsRepo = Database.GetRepo<TagsRepository, Tag>();

            if (string.IsNullOrEmpty(postModel.Content))
            {
                return DataServiceResult.Failed("Пустой контент");
            }
            var tags = (List<Tag>)null;
            if(postModel.Tags != null)
            {
                tags = Mapper.Map<List<Tag>>(postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)));
                postModel.Tags = null;
            }
            var post = Mapper.Map<Post>(postModel);

            post.Id = Guid.NewGuid();
            post.UserId = userId;
            post.LastEdit = DateTime.UtcNow;
            post.Rating = 0;

            if (post.Type == PostTypes.Published)
                post.PublishDate = DateTime.UtcNow;
            else
                post.Type = PostTypes.Drafts;

            var result = DataServiceResult.Failed("Внутренняя ошибка");
            switch (post.ContentType)
            {
                case PostContentTypes.Image:
                    result = CreateImage(post);
                    break;
                case PostContentTypes.ImageCollection:
                    result = CreateImageCollection(post);
                    break;
                case PostContentTypes.Document:
                    result = CreateDocument(post);
                    break;
            }
            if (!result.Succeeded)
                return result;
            using(var transaction = repo.BeginTransaction())
            {
                try
                {
                    await repo.AddAsync(post);
                    if(tags != null)
                        await tagsRepo.AddWithPostAsync(tags, post.Id);
                    transaction.Commit();
                    return DataServiceResult.Success();
                }
                catch(Exception e)
                {
                    transaction.Rollback();
                    return CommonError("Ошибка при добавлении работы", e);
                }
            }
        }

        private DataServiceResult CreateImageCollection(Post post)
        {
            try
            {
                var tuple = ImagesHelper.CreateImage(imagesFolder, post.Content);
                post.Content = tuple.vPath;
                post.Content_50 = tuple.compressedVPath;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении изображений", e);
            }

            var lastPost = 0;
            foreach (var item in post.Collection)
            {
                try
                {
                    var tuple = ImagesHelper.CreateImage(imagesFolder, item.ContentSrc);
                    item.ContentSrc = tuple.vPath;
                    item.ContentSrc_50 = tuple.compressedVPath;
                    lastPost++;
                }
                catch (Exception e)
                {
                    for (var j = 0; j < lastPost; j++)
                    {
                        FilesHelper.DeleteFile(post.Collection[j].ContentSrc);
                        FilesHelper.DeleteFile(post.Collection[j].ContentSrc_50);
                    }
                    return CommonError("Ошибка при добавлении изображений", e);
                }
            }
            return DataServiceResult.Success(post);
        }

        private DataServiceResult CreateDocument(Post post)
        {
            if (string.IsNullOrEmpty(post.Content))
                return DataServiceResult.Failed("Контент не может быть пустым.");

            if (!string.IsNullOrEmpty(post.Cover))
            {
                try
                {
                    var tuple = ImagesHelper.CreateImage(imagesFolder, post.Cover);
                    post.Cover = tuple.vPath;
                    post.Cover_50 = tuple.compressedVPath;
                }
                catch(Exception e)
                {
                    return CommonError("Ошибка при создании документа", e);
                }
            }
            try
            {
                post.Content = FilesHelper.CreateDocument(documentsFolder, post.Content);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при создании документа", e);
            }
            return Success;
        }

        private DataServiceResult CreateImage(Post post)
        {
            try
            {
                post.Content = FilesHelper.CreateImage(imagesFolder, post.Content);

                var fullCPath = HostingEnvironment.MapPath(post.Content);
                ImagesHelper.Compressed.Compress(fullCPath);
                post.Content_50 = ImagesHelper.Compressed.CompressedVirtualPath(fullCPath);

                return Success;
            }
            catch(Exception e)
            {
                FilesHelper.DeleteFile(post.Content);
                FilesHelper.DeleteFile(post.Content_50);
                return CommonError("Ошибка при создании изображения", e);
            }
        }

        public async Task<DataServiceResult> DeletePostAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var post = await repo.GetWithCollectionAsync(postId);
            try
            {
                switch (post.ContentType)
                {
                    case PostContentTypes.Image:
                        ImagesHelper.DeleteImage(post.Content, post.Content_50);
                        break;
                    case PostContentTypes.ImageCollection:
                        ImagesHelper.DeleteImage(post.Content, post.Content_50);
                        foreach(var colItem in post.Collection)
                        {
                            ImagesHelper.DeleteImage(colItem.ContentSrc, colItem.ContentSrc_50);
                        }
                        break;
                    case PostContentTypes.Document:
                        if(!string.IsNullOrEmpty(post.Cover))
                            ImagesHelper.DeleteImage(post.Cover, post.Cover_50);
                        FilesHelper.DeleteFile(post.Content);
                        break;
                }
                await repo.RemoveAsync(postId);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении поста", e);
            }
        }
        /// <summary>
        /// Обновляет пост, PostModel.Content может быть null(когда не было изменений), если это файл
        /// </summary>
        /// <param name="postModel">модель</param>
        /// <returns></returns>
        public async Task<DataServiceResult> UpdatePostAsync(PostModel postModel)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var currentPost = await repo.GetWithCollectionAsync(postModel.Id);
            if (currentPost.Type == PostTypes.Blocked)
                return DataServiceResult.Failed("Заблокированный пост нельзя обновлять");
            if (currentPost.PublishDate != null && (DateTime.UtcNow - currentPost.PublishDate).Value.TotalDays > 3)
                return DataServiceResult.Failed("Время для редактирования истекло");

            var result = DataServiceResult.Failed("Внутренняя ошибка");
            switch (postModel.ContentType)
            {
                case PostContentTypes.Image:
                    result = UpdateImage(currentPost, postModel);
                    break;
                case PostContentTypes.Document:
                    result = UpdateDocument(currentPost, postModel);
                    break;
                case PostContentTypes.ImageCollection:
                    result = UpdateImage(currentPost, postModel);
                    if (!result.Succeeded)
                        break;
                    result = UpdateImageCollection(currentPost, postModel);
                    break;
            }
            if (!result.Succeeded)
                return result;
            currentPost.Description = postModel.Description;
            currentPost.Title = postModel.Header;
            currentPost.LastEdit = DateTime.UtcNow;

            if (currentPost.PublishDate == null && postModel.Type == PostTypes.Published)
            {
                currentPost.PublishDate = DateTime.UtcNow;
                currentPost.Type = PostTypes.Published;
            }
            var tags = postModel.Tags?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new Tag
            {
                Name = x.ToLower()
            }).ToList();
            if(tags != null)
                await Database.GetRepo<TagsRepository, Tag>().AddWithPostAsync(tags, currentPost.Id);
            currentPost.Tags = null;
            await repo.UpdateAsync(currentPost);

            return Success;
        }
        /// <summary>
        /// Удаление, создание изображения
        /// </summary>
        /// <param name="post">Старый пост</param>
        /// <param name="model">Модель с новыми данными</param>
        /// <returns></returns>
        private DataServiceResult UpdateImage(Post post, PostModel model)
        {
            if (post.Content != model.Content)
            {
                try
                {
                    var tuple = ImagesHelper.UpdateImage(post.Content, post.Content_50, model.Content);
                    post.Content = tuple.vPath;
                    post.Content_50 = tuple.compressedVPath;

                    return Success;
                }
                catch(Exception e)
                {
                    return CommonError("Ошибка при обновлении изображения", e);
                }
            }
            return Success;
        }
        /// <summary>
        /// Обновляем контент документа, а также обложку
        /// </summary>
        /// <param name="post"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private DataServiceResult UpdateDocument(Post post, PostModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Content))
                FilesHelper.UpdateDocument(post.Content, model.Content);

            if(!string.Equals(model.Cover, post.Cover))
            {
                try
                {
                    //Заменяем
                    if (!string.IsNullOrEmpty(model.Cover) && !string.IsNullOrEmpty(post.Cover))
                    {
                        var tuple = ImagesHelper.UpdateImage(post.Cover, post.Cover_50, model.Cover);
                        post.Cover = tuple.vPath;
                        post.Cover_50 = tuple.compressedVPath;
                    }
                    //Добавляем
                    else if (!string.IsNullOrEmpty(model.Cover) && string.IsNullOrEmpty(post.Cover))
                    {
                        var tuple = ImagesHelper.CreateImage(imagesFolder, model.Cover);
                        post.Cover = tuple.vPath;
                        post.Cover_50 = tuple.compressedVPath;
                    }
                    //Удаляем
                    else if(string.IsNullOrEmpty(model.Cover) && !string.IsNullOrEmpty(post.Cover))
                    {
                        ImagesHelper.DeleteImage(post.Cover, post.Cover_50);
                        post.Cover = null;
                    }
                }
                catch(Exception e)
                {
                    return CommonError("Ошибка при обновлении документа", e);
                }
            }
            return Success;
        }
        /// <summary>
        /// Обновляем элементы коллекции
        /// </summary>
        /// <param name="post"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private DataServiceResult UpdateImageCollection(Post post, PostModel model)
        {
            var itemsToUpdate = post.Collection.Where(x => model.Collection.Any(y => y.Id == x.Id));
            var itemsToAdd = model.Collection.Where(x => x.Id == Guid.Empty && !post.Collection.Any(y => y.Id == x.Id));
            var itemsToDel = post.Collection.Except(itemsToUpdate);
            try
            {
                foreach (var postItem in itemsToUpdate)
                {
                    var modelItem = model.Collection.First(x => x.Id == postItem.Id);
                    postItem.Description = modelItem.Description;
                    if (postItem.ContentSrc != modelItem.Content)
                    {
                        var tuple = ImagesHelper.UpdateImage(postItem.ContentSrc, postItem.ContentSrc_50, modelItem.Content);
                        postItem.ContentSrc = tuple.vPath;
                        postItem.ContentSrc_50 = tuple.compressedVPath;
                    }
                }
                foreach (var modelItem in itemsToAdd)
                {
                    modelItem.Id = Guid.NewGuid();
                    var postItem = Mapper.Map<PostCollectionItem>(modelItem);
                    var tuple = ImagesHelper.CreateImage(imagesFolder, modelItem.Content);
                    postItem.PostId = post.Id;
                    postItem.ContentSrc = tuple.vPath;
                    postItem.ContentSrc_50 = tuple.compressedVPath;
                    post.Collection.Add(postItem);
                }
                foreach(var postItem in itemsToDel)
                {
                    ImagesHelper.DeleteImage(postItem.ContentSrc, postItem.ContentSrc_50);
                }
                post.Collection = post.Collection.Except(itemsToDel).ToList();
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при обновлении коллекции", e);
            }
            return Success;
        }
        /// <summary>
        /// Получить пост с тегами(для редактирования поста)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<PostModel> GetWithTagsAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var post = await repo.GetWithTagsAsync(postId);
            post.Tags = post.Tags.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            
            var postModel = Mapper.Map<PostModel>(post);
            postModel.User = new UserShortModel
            {
                Id = post.UserId
            };

            return postModel;
        }
        /// <summary>
        /// Получить с тегами и пользователем(для показа поста)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<PostModel> GetWithTagsUserAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var post = await repo.GetWithTagsAsync(postId);
            if (post == null)
                return null;
            post.Tags = post.Tags.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            var user = await _userManager.FindByIdAsync(post.UserId);
            var postModel = Mapper.Map<PostModel>(post);
            var userModel = Mapper.Map<UserShortModel>(user);

            if (post.ContentType == PostContentTypes.Document)
            {
                //Заменяем путь к документу на содержание
                postModel.Content = await FilesHelper.ReadDocumentAsync(post.Content);
            }
            postModel.User = userModel;
            postModel.CommentsCount = await Database.GetRepo<CommentsRepository, Comment>().GetPostCommentsCountAsync(postId);

            return postModel;
        }
        public async Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userName, string currentUserId, SortFilter sort, PostTypes type)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var user = await _userManager.FindByNameAsync(userName);
            var posts = await repo.GetByUserAsync(user.Id, type, sort);

            var currentUser = string.IsNullOrEmpty(currentUserId) ? null : await _userManager.FindByIdAsync(currentUserId);
            const int minChars = 400;

            var postModels = Mapper.Map<IEnumerable<ProfilePostModel>>(posts, opts =>
            {
                opts.Items.Add("currentUser", currentUser);
                opts.Items.Add("minChars", minChars);
            });
            
            return postModels;
        }

        public Task AddViews(Guid postId)
        {
            return Database.GetRepo<PostsRepository, Post>().AddView(postId);
        }
        public async Task<DataServiceResult> PublishPost(Guid postId)
        {
            try
            {
                await Database.GetRepo<PostsRepository, Post>().PublishPost(postId, DateTime.UtcNow);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при публикации поста: " + e.Message);
                return DataServiceResult.Failed("Ошибка при публикации поста");
            }
        }

        public async Task<IEnumerable<TopPostModel>> GetTopAsync(PostTopFilterModel filter, string currentUserId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var tagsRepo = Database.GetRepo<TagsRepository, Tag>();
            var commentsRepo = Database.GetRepo<CommentsRepository, Comment>();

            const int range = 25;
            var repoFilter = new PostTopFilter(range, filter.Page)
            {
                MaxDate = filter.InitialDate,
                OnlyFollowings = PostUserFilter.OnlyFollowings == filter.PostUserFilter,
                PostName = filter.PostName,
                Tags = filter.Tags?.Split(',')
            };

            var currentDate = DateTime.UtcNow;
            switch (filter.PostTimeFilter)
            {
                case PostTimeFilter.All:
                    repoFilter.MinDate = new DateTime(1800, 1, 1);
                    break;
                case PostTimeFilter.Day:
                    repoFilter.MinDate = currentDate.AddDays(-1);
                    break;
                case PostTimeFilter.Month:
                    repoFilter.MinDate = currentDate.AddMonths(-1);
                    break;
                case PostTimeFilter.Week:
                    repoFilter.MinDate = currentDate.AddDays(-7);
                    break;
                default:
                    throw new NullReferenceException("Не задан фильтр времени при поиске поста");
            }

            var posts = await repo.GetByFilterAsync(repoFilter);
            var currentUser = string.IsNullOrEmpty(currentUserId) ? null : await _userManager.FindByIdAsync(currentUserId);
            const int minChars = 400;

            var postModels = Mapper.Map<IEnumerable<TopPostModel>>(posts, opts =>
            {
                opts.Items.Add("currentUser", currentUser);
                opts.Items.Add("minChars", minChars);
            });
            
            return postModels;
        }

        public async Task<GalleryModel> GetGalleryByUserAsync(string userId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var posts = await repo.GetGalleryByUserAsync(userId);

            return new GalleryModel
            {
                Items = Mapper.Map<GalleryItemModel[]>(posts)
            };
        }
    }
}