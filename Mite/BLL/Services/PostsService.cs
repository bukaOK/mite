﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Helpers;
using Mite.Models;
using Mite.BLL.IdentityManagers;
using Mite.Enums;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Hosting;
using System.Net;
using System.IO;
using Mite.BLL.DTO;
using NLog;

namespace Mite.BLL.Services
{
    public interface IPostsService
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
        Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userId, SortFilter sort, PostTypes type);
        Task<IEnumerable<GalleryPostModel>> GetGalleryByUserAsync(string userId);
        /// <summary>
        /// Добавляем к посту один просмотр
        /// </summary>
        /// <param name="postId">Id поста</param>
        /// <returns></returns>
        Task AddViews(Guid postId);
        Task PublishPost(Guid postId);
        Task<IEnumerable<TopPostModel>> GetTopAsync(string[] tags, string postName, SortFilter sortFilter,
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, string currentUserId, int page);
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
            var post = await Database.PostsRepository.GetAsync(postId);
            var postModel = Mapper.Map<PostModel>(post);

            postModel.IsImage = FilesHelper.IsPath(postModel.Content);
            postModel.User = new UserShortModel
            {
                Id = post.UserId
            };
            return postModel;
        }

        public async Task<IdentityResult> AddPostAsync(PostModel postModel, string userId)
        {
            if (postModel.IsImage)
            {
                if(postModel.Content == null)
                {
                    return IdentityResult.Failed("Изображение не найдено.");
                }
                
                postModel.Content = FilesHelper.CreateImage(imagesFolder, postModel.Content);
                //Создаем сжатую копию изображения
                var img = new ImageDTO(postModel.Content, imagesFolder);
                img.Compress();
            }
            else
            {
                if(!string.IsNullOrEmpty(postModel.Cover))
                {
                    postModel.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
                if(string.IsNullOrEmpty(postModel.Content))
                {
                    return IdentityResult.Failed("Контент не может быть пустым.");
                }
                postModel.Content = FilesHelper.CreateDocument(documentsFolder, postModel.Content);
            }
            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var post = Mapper.Map<Post>(postModel);

            post.Id = Guid.NewGuid();
            post.UserId = userId;
            post.LastEdit = DateTime.UtcNow;
            post.Rating = 0;
            if (postModel.IsPublished)
            {
                post.PublishDate = DateTime.UtcNow;
            }

            await Database.PostsRepository.AddAsync(post);
            foreach (var tag in post.Tags)
                tag.Name = tag.Name.ToLower();
            await Database.TagsRepository.AddWithPostAsync(post.Tags, post.Id);
            return IdentityResult.Success;
        }

        public async Task<DataServiceResult> DeletePostAsync(Guid postId)
        {
            var post = await Database.PostsRepository.GetAsync(postId);
            if (post.IsImage)
            {
                var img = new ImageDTO(post.Content, imagesFolder);
                if (img.IsCompressedExists)
                {
                    FilesHelper.DeleteFile(img.CompressedVirtualPath);
                }
            }
            else if (!string.IsNullOrEmpty(post.Cover))
            {
                FilesHelper.DeleteFile(post.Cover);
            }
            FilesHelper.DeleteFile(post.Content);
            await Database.PostsRepository.RemoveAsync(postId);
            return DataServiceResult.Success();
        }
        /// <summary>
        /// Обновляет пост, PostModel.Content может быть null(когда не было изменений), если это файл
        /// </summary>
        /// <param name="postModel">модель</param>
        /// 
        /// <returns></returns>
        public async Task<IdentityResult> UpdatePostAsync(PostModel postModel)
        {
            var currentPost = await Database.PostsRepository.GetAsync(postModel.Id);
            if (currentPost.Blocked)
            {
                return IdentityResult.Failed("Заблокированный пост нельзя обновлять");
            }
            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var post = Mapper.Map<Post>(postModel);
            post.Rating = currentPost.Rating;
            post.Views = currentPost.Views;

            if (postModel.IsImage && postModel.Content != currentPost.Content)
            {
                if (postModel.Content == null)
                    return IdentityResult.Failed("Изображение не может быть пустым");
                FilesHelper.DeleteFile(currentPost.Content);
                post.Content = FilesHelper.CreateImage(imagesFolder, postModel.Content);
            }
            else if(!postModel.IsImage)
            {
                if(!string.IsNullOrWhiteSpace(postModel.Content))
                    FilesHelper.UpdateDocument(currentPost.Content, postModel.Content);

                post.Content = currentPost.Content;
                if(!string.IsNullOrWhiteSpace(postModel.Cover) && postModel.Cover != currentPost.Cover &&
                    currentPost.Cover != null)
                {
                    FilesHelper.DeleteFile(currentPost.Cover);
                    post.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
                else if(string.IsNullOrEmpty(postModel.Cover) && !string.IsNullOrEmpty(currentPost.Cover))
                {
                    FilesHelper.DeleteFile(currentPost.Cover);
                    post.Cover = null;
                }
                else if(!string.IsNullOrEmpty(postModel.Cover) && string.IsNullOrEmpty(currentPost.Cover))
                {
                    post.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
            }
            post.LastEdit = DateTime.UtcNow;
            if(currentPost.PublishDate == null && postModel.IsPublished)
            {
                post.PublishDate = DateTime.UtcNow;
            }
            else
            {
                post.PublishDate = currentPost.PublishDate;
            }
            foreach (var tag in post.Tags)
                tag.Name = tag.Name.ToLower();
            await Database.TagsRepository.AddWithPostAsync(post.Tags, post.Id);
            await Database.PostsRepository.UpdateAsync(post);

            return IdentityResult.Success;
        }
        /// <summary>
        /// Получить пост с тегами(для редактирования поста)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<PostModel> GetWithTagsAsync(Guid postId)
        {
            var post = await Database.PostsRepository.GetWithTagsAsync(postId);
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
            var post = await Database.PostsRepository.GetWithTagsAsync(postId);
            var user = await _userManager.FindByIdAsync(post.UserId);
            var postModel = Mapper.Map<PostModel>(post);
            var userModel = Mapper.Map<UserShortModel>(user);

            postModel.User = userModel;
            postModel.CommentsCount = await Database.CommentsRepository.GetPostCommentsCountAsync(postId);

            return postModel;
        }
        public async Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userId, SortFilter sort, PostTypes type)
        {
            IEnumerable<Post> posts;
            switch (type)
            {
                case PostTypes.Drafts:
                    posts = await Database.PostsRepository.GetByUserAsync(userId, false, false);
                    break;
                case PostTypes.Published:
                    posts = await Database.PostsRepository.GetByUserAsync(userId, true, false);
                    break;
                case PostTypes.Blocked:
                    posts = await Database.PostsRepository.GetByUserAsync(userId, true, true);
                    break;
                default:
                    return null;
            }
            const int minChars = 400;
            foreach (var post in posts)
            {
                if (!post.IsImage)
                {
                    try
                    {
                        post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Ошибка при чтении файла в топе, имя файла: {post.Content}, Ошибка : {e.Message}");
                        post.Content = "Ошибка при чтении файла.";
                    }
                }
                else
                {
                    var img = new ImageDTO(post.Content, imagesFolder);
                    if (img.IsCompressedExists)
                    {
                        post.Content = img.CompressedVirtualPath;
                    }
                }
                if(!string.IsNullOrEmpty(post.Description) && post.Description.Length > 100)
                {
                    var description = post.Description;
                    //Находим последний пробел, обрезаем до него
                    description = description.Substring(0, 100);
                    var lastSpace = description.LastIndexOf(' ');
                    description = description.Substring(0, lastSpace);
                    //Убираем последний символ, если это знак препинания
                    var lastChar = description[description.Length - 1];
                    if (lastChar == ',' || lastChar == '.' || lastChar == '-')
                    {
                        description = description.Substring(0, description.Length - 1);
                    }
                    post.Description = description + "...";
                }
            }
            var postModels = Mapper.Map<IEnumerable<ProfilePostModel>>(posts);
            switch (sort)
            {
                case SortFilter.New:
                    return postModels.OrderByDescending(x => x.LastEdit)
                        .ThenByDescending(x => x.Rating);
                case SortFilter.Old:
                    return postModels.OrderBy(x => x.LastEdit)
                        .ThenByDescending(x => x.Rating);
                case SortFilter.Popular:
                    return postModels.OrderByDescending(x => x.Rating)
                        .ThenBy(x => x.LastEdit);
                default:
                    throw new ArgumentException("Неизвестный тип сортировки");
            }
        }

        public Task AddViews(Guid postId)
        {
            return Database.PostsRepository.AddView(postId);
        }
        public Task PublishPost(Guid postId)
        {
            return Database.PostsRepository.PublishPost(postId, DateTime.UtcNow);
        }

        public async Task<IEnumerable<TopPostModel>> GetTopAsync(string[] tagsNames, string postName, SortFilter sortFilter, 
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, string currentUserId, int page)
        {
            var currentDate = DateTime.Now;
            DateTime minDate;
            var onlyFollowings = PostUserFilter.OnlyFollowings == postUserFilter;
            const int range = 9;
            var offset = (page - 1) * range;

            switch (postTimeFilter)
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
            if (!string.IsNullOrWhiteSpace(postName) && tagsNames.Length > 0)
            {
                //Находим посты по тегам и имени работы
                posts = await Database.PostsRepository.GetByPostNameAndTagsAsync(postName, tagsNames, minDate, 
                    onlyFollowings, currentUserId, sortFilter, offset, range);
            }
            else if (!string.IsNullOrEmpty(postName))
            {
                posts = await Database.PostsRepository.GetByPostNameAsync(postName, minDate,
                    onlyFollowings, currentUserId, sortFilter, offset, range);
            }
            else if(tagsNames.Length > 0)
            {
                posts = await Database.PostsRepository.GetByTagsAsync(tagsNames, minDate,
                    onlyFollowings, currentUserId, sortFilter, offset, range);
            }
            else
            {
                posts = await Database.PostsRepository.GetByFilterAsync(minDate, onlyFollowings, currentUserId,
                    sortFilter, offset, range);
            }

            var postTags = await Database.TagsRepository.GetByPostsAsync(posts.Select(x => x.Id));
            const int minChars = 400;
            foreach (var post in posts)
            {
                if (!post.IsImage)
                {
                    try
                    {
                        post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Ошибка при чтении файла в топе, имя файла: {post.Content}, Ошибка : {e.Message}");
                        post.Content = "Ошибка при чтении файла.";
                    }
                }
                else
                {
                    var img = new ImageDTO(post.Content, imagesFolder);
                    if (img.IsCompressedExists)
                    {
                        post.Content = img.CompressedVirtualPath;
                    }
                }
                var avatarImg = new ImageDTO(post.User.AvatarSrc, imagesFolder);
                if (avatarImg.IsCompressedExists)
                {
                    post.User.AvatarSrc = avatarImg.CompressedVirtualPath;
                }
                post.Tags = postTags.Where(x => x.Posts.Any(y => y.Id == post.Id)).ToList();
            }
            var postModels = Mapper.Map<IEnumerable<TopPostModel>>(posts);
            var postsWithCommentsCount = await Database.CommentsRepository.GetPostsCommentsCountAsync(postModels.Select(x => x.Id));

            int commentsCount;
            foreach (var postModel in postModels)
            {
                var hasComments = postsWithCommentsCount.TryGetValue(postModel.Id, out commentsCount);
                postModel.CommentsCount = hasComments ? commentsCount : 0;
            }
            return postModels;
        }
        /// <summary>
        /// Объединяем посты с тегами, на основе Id поста
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private IEnumerable<Post> ConcatTagWithPosts(IEnumerable<Post> posts, IEnumerable<Tag> tags)
        {
            foreach(var post in posts)
            {
                post.Tags = tags.Where(x => x.Posts.Any(y => y.Id == post.Id)).ToList();
            }
            return posts;
        }

        public async Task<IEnumerable<GalleryPostModel>> GetGalleryByUserAsync(string userId)
        {
            var posts = await Database.PostsRepository.GetGalleryByUserAsync(userId);
            return Mapper.Map<IEnumerable<GalleryPostModel>>(posts);
        }
    }
}