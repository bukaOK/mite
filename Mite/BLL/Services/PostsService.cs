using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Models;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Enums;
using System.Collections.Generic;
using System.Web.Hosting;
using NLog;
using Mite.DAL.Repositories;
using Mite.BLL.Helpers;
using Mite.DAL.Filters;
using Mite.BLL.Infrastructure;

namespace Mite.BLL.Services
{
    public interface IPostsService : IDataService
    {
        Task<PostModel> GetPostAsync(Guid postId);
        Task<DataServiceResult> AddPostAsync(PostModel postModel, string userId);
        Task<DataServiceResult> DeletePostAsync(Guid postId);
        Task<DataServiceResult> UpdatePostAsync(PostModel postModel);
        /// <summary>
        /// Для редактирования
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<PostModel> GetWithTagsAsync(Guid postId);
        /// <summary>
        /// Возвращает вместе с тегами и владельцем поста(для показа)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<PostModel> GetWithTagsUserAsync(Guid postId, string currentUserId);
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
        Task AddViewsAsync(Guid postId);
        Task<DataServiceResult> PublishPost(Guid postId);
        Task<IEnumerable<TopPostModel>> GetTopAsync(PostTopFilterModel filter, string currentUserId);
    }
    public class PostsService : DataService, IPostsService
    {
        private readonly AppUserManager _userManager;
        private readonly IWatermarkService watermarkService;
        private readonly IProductsService productsService;
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsService(IUnitOfWork unitOfWork, AppUserManager userManager, ILogger logger, IWatermarkService watermarkService, 
            IProductsService productsService) : base(unitOfWork, logger)
        {
            _userManager = userManager;
            this.watermarkService = watermarkService;
            this.productsService = productsService;
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
                return DataServiceResult.Failed("Пустой контент");

            var tags = (List<Tag>)null;
            if (postModel.Tags != null)
            {
                tags = Mapper.Map<List<Tag>>(postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)));
                postModel.Tags = null;
            }
            var post = Mapper.Map<Post>(postModel);
            using (var transaction = repo.BeginTransaction())
            {
                if (postModel.Watermark != null)
                {
                    var result = await watermarkService.AddAsync(postModel.Watermark);
                    if (result.Succeeded)
                        post.WatermarkId = result.ResultData as Guid?;
                    else
                    {
                        transaction.Rollback();
                        return result;
                    }
                }
                if (postModel.Product != null)
                {
                    var result = await productsService.AddAsync(postModel.Product);
                    if (result.Succeeded)
                        post.ProductId = result.ResultData as Guid?;
                    else
                    {
                        transaction.Rollback();
                        return result;
                    }
                }
                post.Id = Guid.NewGuid();
                post.UserId = userId;
                post.LastEdit = DateTime.UtcNow;
                post.Rating = 0;

                if (post.Type == PostTypes.Published)
                    post.PublishDate = DateTime.UtcNow;
                else
                    post.Type = PostTypes.Drafts;

                switch (post.ContentType)
                {
                    case PostContentTypes.Image:
                        PostsHelper.CreateImage(post);
                        break;
                    case PostContentTypes.ImageCollection:
                    case PostContentTypes.Comics:
                        PostsHelper.CreatePostCollection(post, post.ContentType);
                        break;
                    case PostContentTypes.Document:
                        PostsHelper.CreateDocument(post);
                        break;
                    default:
                        throw new ArgumentException("Неизвестный тип работы");
                }
            
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

        public async Task<DataServiceResult> DeletePostAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var post = await repo.GetWithCollectionsAsync(postId);
            try
            {
                switch (post.ContentType)
                {
                    case PostContentTypes.Image:
                        FilesHelper.DeleteFile(post.Content);
                        break;
                    case PostContentTypes.ImageCollection:
                        FilesHelper.DeleteFile(post.Content);
                        FilesHelper.DeleteFiles(post.Collection.Select(x => x.ContentSrc));
                        //FilesHelper.DeleteFiles(post.Collection.Select(x => x.ContentSrc_50));
                        break;
                    case PostContentTypes.Document:
                        if(!string.IsNullOrEmpty(post.Cover))
                            FilesHelper.DeleteFile(post.Cover);
                        FilesHelper.DeleteFile(post.Content);
                        break;
                    case PostContentTypes.Comics:
                        FilesHelper.DeleteFile(post.Content);
                        FilesHelper.DeleteFiles(post.ComicsItems.Select(x => x.ContentSrc));
                        //FilesHelper.DeleteFiles(post.ComicsItems.Select(x => x.ContentSrc_50));
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

            var currentPost = await repo.GetWithCollectionsAsync(postModel.Id);
            if (currentPost.Type == PostTypes.Blocked)
                return DataServiceResult.Failed("Заблокированную работу нельзя обновлять");
            //Какие то элементы могут обновляться в любое время
            //Главное изображение обновляться не может
            //Для некоторых элементов ставим срок обновления на 3 дня
            var updateTimeExpired = currentPost.PublishDate != null && (DateTime.UtcNow - currentPost.PublishDate).Value.TotalDays > 3;

            using(var transaction = repo.BeginTransaction())
            {
                //Фактически это значит что человек нажал на кнопку удаления водяного знака, костыль но...
                if(postModel.WatermarkId == null)
                {
                    if (postModel.Watermark != null)
                    {
                        if (currentPost.WatermarkId != null)
                        {
                            var remWatId = (Guid)currentPost.WatermarkId;
                            currentPost.WatermarkId = null;
                            await repo.UpdateAsync(currentPost);
                            var remResult = await watermarkService.RemoveAsync(remWatId);
                            if (!remResult.Succeeded)
                            {
                                transaction.Rollback();
                                return remResult;
                            }
                        }
                        var result = await watermarkService.AddAsync(postModel.Watermark);
                        if (result.Succeeded)
                            currentPost.WatermarkId = result.ResultData as Guid?;
                        else
                        {
                            transaction.Rollback();
                            return result;
                        }
                    }
                    else if (postModel.Watermark == null && currentPost.WatermarkId != null)
                    {
                        var remWatId = currentPost.WatermarkId;
                        currentPost.WatermarkId = null;
                        await repo.UpdateAsync(currentPost);
                        var result = await watermarkService.RemoveAsync((Guid)remWatId);
                        if (!result.Succeeded)
                        {
                            transaction.Rollback();
                            return result;
                        }
                    }
                }
                if (postModel.Product != null && currentPost.ProductId == null)
                {
                    var result = await productsService.AddAsync(postModel.Product);
                    if (result.Succeeded)
                        currentPost.ProductId = result.ResultData as Guid?;
                    else
                    {
                        transaction.Rollback();
                        return result;
                    }
                }
                else if (postModel.Product == null && currentPost.ProductId != null)
                {
                    var result = await productsService.RemoveAsync((Guid)currentPost.ProductId);
                    if (result.Succeeded)
                        currentPost.ProductId = null;
                    else
                    {
                        transaction.Rollback();
                        return result;
                    }
                }
                else if(postModel.Product != null && currentPost.ProductId != null)
                {
                    var result = await productsService.UpdateAsync(postModel.Product);
                    if (!result.Succeeded)
                    {
                        transaction.Rollback();
                        return result;
                    }
                }
                try
                {
                    if (!updateTimeExpired)
                    {
                        switch (postModel.ContentType)
                        {
                            //case PostContentTypes.Image:
                            //    PostsHelper.UpdateImage(currentPost, postModel);
                            //    break;
                            case PostContentTypes.Document:
                                PostsHelper.UpdateDocument(currentPost, postModel);
                                break;
                            case PostContentTypes.ImageCollection:
                                //PostsHelper.UpdateImage(currentPost, postModel);
                                PostsHelper.UpdateImageCollection(currentPost, postModel);
                                break;
                            case PostContentTypes.Comics:
                                //PostsHelper.UpdateImage(currentPost, postModel);
                                PostsHelper.UpdateComicsItems(currentPost, postModel);
                                break;
                        }
                    }
                    currentPost.Description = postModel.Description;
                    currentPost.Title = postModel.Header;
                    currentPost.UseWatermarkForCols = postModel.UseWatermarkForCols;
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
                    if (tags != null)
                        await Database.GetRepo<TagsRepository, Tag>().AddWithPostAsync(tags, currentPost.Id);
                    currentPost.Tags = null;
                    await repo.UpdateAsync(currentPost);

                    transaction.Commit();
                    return Success;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return CommonError("Ошибка при обновлении", e);
                }
            }
        }
        
        public async Task<PostModel> GetWithTagsAsync(Guid postId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var post = await repo.GetWithTagsAsync(postId);
            if (post == null)
                return null;
            post.Tags = post.Tags.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            
            var postModel = Mapper.Map<PostModel>(post);
            postModel.User = new UserShortModel
            {
                Id = post.UserId
            };

            return postModel;
        }
        
        public async Task<PostModel> GetWithTagsUserAsync(Guid postId, string currentUserId)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();
            var favoritesRepo = Database.GetRepo<FavoritePostsRepository, FavoritePost>();

            var post = await repo.GetFullAsync(postId);
            if (post == null)
                return null;
            var user = await _userManager.FindByIdAsync(post.UserId);
            var postModel = Mapper.Map<PostModel>(post);

            if (post.ContentType == PostContentTypes.Document)
            {
                //Заменяем путь к документу на содержание
                postModel.Content = await FilesHelper.ReadDocumentAsync(post.Content);
            }
            postModel.User = Mapper.Map<UserShortModel>(user);
            postModel.IsFavorite = await favoritesRepo.IsFavoriteAsync(postId, currentUserId);
            postModel.FavoriteCount = await favoritesRepo.FavoriteCountAsync(postId);
            postModel.CommentsCount = await Database.GetRepo<CommentsRepository, Comment>().GetPostCommentsCountAsync(postId);

            if(postModel.Product != null && post.ProductId != null)
            {
                postModel.Product.IsBought = await Database.GetRepo<PurchaseRepository, Purchase>().IsBuyerAsync(currentUserId, (Guid)post.ProductId);
            }
            postModel.Content = $"/images/post/{post.Id}?watermark={post.WatermarkId != null}&resize=false";
            return postModel;
        }
        public async Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userName, string currentUserId, SortFilter sort, PostTypes type)
        {
            var repo = Database.GetRepo<PostsRepository, Post>();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new DataServiceException("Неизвестный пользователь");
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

        public Task AddViewsAsync(Guid postId)
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
                PostName = filter.Input?.Split('#').FirstOrDefault(),
                Tags = filter.Input?.Split('#').Skip(1).ToArray(),
                SortType = filter.SortFilter,
                PostType = PostTypes.Published,
                CurrentUserId = currentUserId
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
            //var user = await _userManager.FindByIdAsync(currentUserId);
            //if (user.ShowOnlyFollowings)
            //    repoFilter.FollowingTags = (await tagsRepo.GetForUserAsync(currentUserId)).Select(x => x.Id).ToArray();
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