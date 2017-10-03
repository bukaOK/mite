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
using Microsoft.AspNet.Identity;
using System.Web.Hosting;
using NLog;
using Mite.DAL.Repositories;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;

namespace Mite.BLL.Services
{
    public interface IPostsService : IDataService
    {
        Task<PostModel> GetPostAsync(Guid postId);
        Task<IdentityResult> AddPostAsync(PostModel postModel, string userId);
        Task<DataServiceResult> DeletePostAsync(Guid postId);
        Task<IdentityResult> UpdatePostAsync(PostModel postModel);
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
        private readonly ILogger logger;
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsService(IUnitOfWork unitOfWork, AppUserManager userManager, ILogger logger) : base(unitOfWork)
        {
            _userManager = userManager;
            this.logger = logger;
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

        public async Task<IdentityResult> AddPostAsync(PostModel postModel, string userId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var tagsRepo = Database.GetRepo<TagsRepository, Tag>();

            if (string.IsNullOrEmpty(postModel.Content))
            {
                return IdentityResult.Failed("Пустой контент");
            }
            switch (postModel.ContentType)
            {
                case PostContentTypes.Image:
                    postModel.Content = FilesHelper.CreateImage(imagesFolder, postModel.Content);
                    ImagesHelper.Compressed.Compress(HostingEnvironment.MapPath(postModel.Content));
                    break;
                case PostContentTypes.ImageCollection:
                    try
                    {
                        //В контенте будет лежать документ с коллекцией
                        postModel.Content = FilesHelper.CreateDocument(PathConstants.VirtualDocumentFolder, postModel.Content);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Ошибка при попытке создания документа с коллекцией: " + e.Message);
                        return IdentityResult.Failed("Внутренняя ошибка");
                    }
                    try
                    {
                        //В обложке лежит главное изображение
                        postModel.Cover = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, postModel.Cover);
                        ImagesHelper.Compressed.Compress(HostingEnvironment.MapPath(postModel.Cover));
                    }
                    catch (Exception e)
                    {
                        logger.Error("Ошибка при попытке создания главного изображения коллекции: " + e.Message);
                        FilesHelper.DeleteFile(postModel.Content);
                        FilesHelper.DeleteFileFull(ImagesHelper.Compressed.CompressedPath(postModel.Content));
                        return IdentityResult.Failed("Внутренняя ошибка");
                    }
                    break;
                case PostContentTypes.Document:
                    if (!string.IsNullOrEmpty(postModel.Cover))
                    {
                        postModel.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                        ImagesHelper.Compressed.Compress(postModel.Cover);
                    }
                    if (string.IsNullOrEmpty(postModel.Content))
                    {
                        return IdentityResult.Failed("Контент не может быть пустым.");
                    }
                    postModel.Content = FilesHelper.CreateDocument(documentsFolder, postModel.Content);
                    break;
            }
            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var tags = Mapper.Map<List<Tag>>(postModel.Tags);

            postModel.Tags = null;
            var post = Mapper.Map<Post>(postModel);

            post.Id = Guid.NewGuid();
            post.UserId = userId;
            post.LastEdit = DateTime.UtcNow;
            post.Rating = 0;

            if(post.ContentType == PostContentTypes.Image)
                post.Content_50 = ImagesHelper.Compressed.CompressedVirtualPath(post.Content);

            if(!string.IsNullOrEmpty(post.Cover))
                post.Cover_50 = ImagesHelper.Compressed.CompressedVirtualPath(post.Cover);

            if (post.Type == PostTypes.Published)
            {
                post.PublishDate = DateTime.UtcNow;
            }

            await repo.AddAsync(post);
            await tagsRepo.AddWithPostAsync(tags, post.Id);
            return IdentityResult.Success;
        }

        public async Task<DataServiceResult> DeletePostAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var post = await repo.GetAsync(postId);
            try
            {
                if (post.ContentType == PostContentTypes.Image)
                {
                    var fullImgPath = HostingEnvironment.MapPath(post.Content);
                    FilesHelper.DeleteFile(ImagesHelper.Compressed.CompressedVirtualPath(fullImgPath));
                }
                if (!string.IsNullOrEmpty(post.Cover))
                {
                    FilesHelper.DeleteFile(post.Cover);
                    FilesHelper.DeleteFile(ImagesHelper.Compressed.CompressedVirtualPath(post.Cover));
                }
                FilesHelper.DeleteFile(post.Content);
                await repo.RemoveAsync(postId);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при удалении поста: " + e.Message);
                return DataServiceResult.Failed("Ошибка при удалении поста");
            }
        }
        /// <summary>
        /// Обновляет пост, PostModel.Content может быть null(когда не было изменений), если это файл
        /// </summary>
        /// <param name="postModel">модель</param>
        /// 
        /// <returns></returns>
        public async Task<IdentityResult> UpdatePostAsync(PostModel postModel)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var currentPost = await repo.GetAsync(postModel.Id);
            if (currentPost.Type == PostTypes.Blocked)
            {
                return IdentityResult.Failed("Заблокированный пост нельзя обновлять");
            }
            currentPost.Description = postModel.Description;
            currentPost.Title = postModel.Header;

            if(postModel.ContentType == PostContentTypes.Document || postModel.ContentType == PostContentTypes.ImageCollection)
            {
                if(!string.IsNullOrWhiteSpace(postModel.Content))
                    FilesHelper.UpdateDocument(currentPost.Content, postModel.Content);

                //Заменяем обложку(главное изображение)
                if(!string.IsNullOrWhiteSpace(postModel.Cover) && postModel.Cover != currentPost.Cover &&
                    currentPost.Cover != null)
                {
                    FilesHelper.DeleteFile(currentPost.Cover);
                    currentPost.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
                //Удаляем обложку(главное изображение)
                else if (string.IsNullOrEmpty(postModel.Cover) && !string.IsNullOrEmpty(currentPost.Cover))
                {
                    FilesHelper.DeleteFile(currentPost.Cover);
                    currentPost.Cover = null;
                }
                //Ставим новую(главное изображение)
                else if (!string.IsNullOrEmpty(postModel.Cover) && string.IsNullOrEmpty(currentPost.Cover))
                {
                    currentPost.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
            }
            currentPost.LastEdit = DateTime.UtcNow;
            if(currentPost.PublishDate == null && postModel.Type == PostTypes.Published)
            {
                currentPost.PublishDate = DateTime.UtcNow;
                currentPost.Type = PostTypes.Published;
            }
            var tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new Tag
            {
                Name = x.ToLower()
            }).ToList();
            await Database.GetRepo<TagsRepository, Tag>().AddWithPostAsync(tags, currentPost.Id);
            await repo.UpdateAsync(currentPost);

            return IdentityResult.Success;
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
            //post.Tags = post.Tags.Where(x => x.IsConfirmed).ToList();
            
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

            postModel.User = userModel;
            postModel.CommentsCount = await Database.GetRepo<CommentsRepository, Comment>().GetPostCommentsCountAsync(postId);

            return postModel;
        }
        public async Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userName, string currentUserId, SortFilter sort, PostTypes type)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var tagsRepo = Database.GetRepo<TagsRepository, Tag>();
            var commentsRepo = Database.GetRepo<CommentsRepository, Comment>();

            var user = await _userManager.FindByNameAsync(userName);
            var currentUser = string.IsNullOrEmpty(currentUserId) ? null : await _userManager.FindByIdAsync(currentUserId);
            var posts = await repo.GetByUserAsync(user.Id, type, sort);

            var postModels = Mapper.Map<IEnumerable<ProfilePostModel>>(posts);
            var postsWithCommentsCount = await commentsRepo.GetPostsCommentsCountAsync(postModels.Select(x => x.Id));
            var postTags = await tagsRepo.GetByPostsAsync(posts.Select(x => x.Id).ToList());

            const int minChars = 400;
            foreach (var postModel in postModels)
            {
                switch (postModel.ContentType)
                {
                    case PostContentTypes.Document:
                        try
                        {
                            postModel.Content = await FilesHelper.ReadDocumentAsync(postModel.Content, minChars);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Ошибка при чтении файла в топе, имя файла: {postModel.Content}, Ошибка : {e.Message}");
                            postModel.Content = "Ошибка при чтении файла.";
                        }
                        break;
                    case PostContentTypes.ImageCollection:
                        //В Cover содержится главное изображение, поэтому меняем местами Content и Cover
                        postModel.Content = postModel.Cover;
                        break;
                    
                }
                var fullPath = HostingEnvironment.MapPath(postModel.Content);
                if (postModel.IsImage)
                {
                    if (ImagesHelper.IsAnimatedImage(fullPath))
                    {
                        postModel.IsGif = true;
                        postModel.FullPath = postModel.Content;
                    }
                    if (ImagesHelper.Compressed.CompressedExists(fullPath))
                    {
                        postModel.Content = ImagesHelper.Compressed.CompressedVirtualPath(fullPath);
                    }
                }
                postModel.Tags = postTags.Where(x => x.Posts.Any(y => y.Id == postModel.Id)).Select(x => x.Name);

                var hasComments = postsWithCommentsCount.TryGetValue(postModel.Id, out int commentsCount);
                postModel.CommentsCount = hasComments ? commentsCount : 0;

                if ((currentUser != null && currentUser.Age >= 18) || !postModel.Tags.Any(tag => tag == "18+"))
                {
                    postModel.ShowAdultContent = true;
                }
            }
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

            var currentDate = DateTime.UtcNow;
            DateTime minDate;
            var onlyFollowings = PostUserFilter.OnlyFollowings == filter.PostUserFilter;

            const int range = 12;
            var offset = (filter.Page - 1) * range;

            switch (filter.PostTimeFilter)
            {
                case PostTimeFilter.All:
                    minDate = new DateTime(1800, 1, 1);
                    break;
                case PostTimeFilter.Day:
                    minDate = currentDate.AddDays(-1);
                    break;
                case PostTimeFilter.Month:
                    minDate = currentDate.AddMonths(-1);
                    break;
                case PostTimeFilter.Week:
                    minDate = currentDate.AddDays(-7);
                    break;
                default:
                    throw new NullReferenceException("Не задан фильтр времени при поиске поста");
            }

            IEnumerable<Post> posts;
            if (!string.IsNullOrWhiteSpace(filter.PostName) && filter.TagNames.Length > 0)
            {
                //Находим посты по тегам и имени работы
                posts = await Database.GetRepo<PostsRepository, Post>().GetByPostNameAndTagsAsync(filter.PostName, filter.TagNames, minDate, 
                    onlyFollowings, currentUserId, filter.SortFilter, offset, range, filter.InitialDate);
            }
            else if (!string.IsNullOrEmpty(filter.PostName))
            {
                posts = await repo.GetByPostNameAsync(filter.PostName, minDate,
                    onlyFollowings, currentUserId, filter.SortFilter, offset, range, filter.InitialDate);
            }
            else if(filter.TagNames.Length > 0)
            {
                posts = await repo.GetByTagsAsync(filter.TagNames, minDate,
                    onlyFollowings, currentUserId, filter.SortFilter, offset, range, filter.InitialDate);
            }
            else
            {
                posts = await repo.GetByFilterAsync(minDate, onlyFollowings, currentUserId,
                    filter.SortFilter, offset, range, filter.InitialDate);
            }

            var postModels = Mapper.Map<IEnumerable<TopPostModel>>(posts);
            var postTags = await tagsRepo.GetByPostsAsync(posts.Select(x => x.Id).ToList());
            var postsWithCommentsCount = await commentsRepo.GetPostsCommentsCountAsync(postModels.Select(x => x.Id));
            var currentUser = string.IsNullOrEmpty(currentUserId) ? null : await _userManager.FindByIdAsync(currentUserId);

            const int minChars = 400;
            string fullImgPath;

            foreach (var postModel in postModels)
            {
                if (!postModel.IsImage)
                {
                    try
                    {
                        postModel.Content = await FilesHelper.ReadDocumentAsync(postModel.Content, minChars);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Ошибка при чтении файла в топе, имя файла: {postModel.Content}, Ошибка : {e.Message}");
                        postModel.Content = "Ошибка при чтении файла.";
                    }
                }
                else
                {
                    //Если Cover не пустой, значит у нас коллекция изображений
                    if (!string.IsNullOrEmpty(postModel.Cover))
                    {
                        //В Cover содержится главное изображение, поэтому меняем местами Content и Cover
                        postModel.Content = postModel.Cover;
                    }
                    fullImgPath = HostingEnvironment.MapPath(postModel.Content);
                    if (ImagesHelper.IsAnimatedImage(fullImgPath))
                    {
                        postModel.IsGif = true;
                        postModel.FullPath = postModel.Content;
                    }

                    if (ImagesHelper.Compressed.CompressedExists(fullImgPath))
                    {
                        postModel.Content = ImagesHelper.Compressed.CompressedVirtualPath(fullImgPath);
                    }
                }
                fullImgPath = HostingEnvironment.MapPath(postModel.User.AvatarSrc);
                if (ImagesHelper.Compressed.CompressedExists(fullImgPath))
                {
                    postModel.User.AvatarSrc = ImagesHelper.Compressed.CompressedVirtualPath(fullImgPath);
                }
                postModel.Tags = postTags.Where(x => x.Posts.Any(y => y.Id == postModel.Id)).Select(x => x.Name);

                var hasComments = postsWithCommentsCount.TryGetValue(postModel.Id, out int commentsCount);
                postModel.CommentsCount = hasComments ? commentsCount : 0;
                if ((currentUser != null && currentUser.Age >= 18) || !postModel.Tags.Any(tag => tag == "18+"))
                {
                    postModel.ShowAdultContent = true;
                }
            }
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