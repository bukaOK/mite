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
using Mite.Enums;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Hosting;
using System.Net;
using System.IO;
using Mite.BLL.DTO;

namespace Mite.BLL.Services
{
    public interface IPostsService
    {
        Task<PostModel> GetPostAsync(Guid postId);
        Task<IdentityResult> AddPostAsync(PostModel postModel, string userId);
        Task DeletePostAsync(Guid postId);
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
        Task<IEnumerable<TopPostModel>> GetTopAsync(string input, SortFilter sortFilter,
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, string currentUserId, int page);
    }
    public class PostsService : DataService, IPostsService
    {
        private readonly AppUserManager _userManager;
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsService(IUnitOfWork unitOfWork, AppUserManager userManager) : base(unitOfWork)
        {
            _userManager = userManager;
        }

        public async Task<PostModel> GetPostAsync(Guid postId)
        {
            var post = await Database.PostsRepository.GetAsync(postId);
            var postModel = Mapper.Map<PostModel>(post);

            postModel.IsImage = FilesHelper.IsPath(postModel.Content);
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
                if(postModel.Cover != null)
                {
                    postModel.Cover = FilesHelper.CreateImage(imagesFolder, postModel.Cover);
                }
                if(postModel.Content == null)
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

            await Database.PostsRepository.AddAsync(post);
            foreach (var tag in post.Tags)
                tag.Name = tag.Name.ToLower();
            await Database.TagsRepository.AddWithPostAsync(post.Tags, post.Id);
            return IdentityResult.Success;
        }

        public async Task DeletePostAsync(Guid postId)
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
            FilesHelper.DeleteFile(post.Content);
            await Database.PostsRepository.RemoveAsync(postId);
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
            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var post = Mapper.Map<Post>(postModel);

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
                if(postModel.Cover == null && !string.IsNullOrEmpty(currentPost.Cover))
                {
                    FilesHelper.DeleteFile(currentPost.Cover);
                }
            }
            post.LastEdit = DateTime.UtcNow;

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
            postModel.CommentsCount = await Database.CommentsRepository.GetPostCommentsCount(postId.ToString());

            return postModel;
        }
        public async Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userId, SortFilter sort, PostTypes type)
        {
            IEnumerable<Post> posts;
            if (type == PostTypes.Drafts)
            {
                posts = await Database.PostsRepository.GetByUserAsync(userId, false);
            }
            else if(type == PostTypes.Published)
            {
                posts = await Database.PostsRepository.GetByUserAsync(userId, true);
            }
            else
            {
                throw new ArgumentException("Не подходящий тип поста");
            }
            const int minChars = 400;
            foreach (var post in posts)
            {
                if (!post.IsImage)
                {
                    post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
                }
                else
                {
                    var img = new ImageDTO(post.Content, imagesFolder);
                    if (img.IsCompressedExists)
                    {
                        post.Content = img.CompressedVirtualPath;
                    }
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
            return Database.PostsRepository.PublishPost(postId);
        }

        public async Task<IEnumerable<TopPostModel>> GetTopAsync(string input, SortFilter sortFilter, 
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
            var posts = new List<Post>();
            //Если строка поиска не пустая, ищем по тегам и по названию поста
            if (!string.IsNullOrWhiteSpace(input))
            {
                //Находим теги по входящей строке
                var inputTags = await Database.TagsRepository.GetByNameAsync(input);
                //Находим посты по входящей строке
                var inputPosts = await Database.PostsRepository.GetByNameAsync(input, true, minDate, onlyFollowings, currentUserId,
                        sortFilter, offset, range);

                if (inputTags.Count() > 0)
                {
                    var tagPosts = await Database.PostsRepository.GetByTagsAsync(inputTags.Select(x => x.Id), inputPosts.Select(x => x.Id),
                        true, minDate, onlyFollowings, currentUserId, sortFilter, offset, range);
                    posts.AddRange(tagPosts);
                    posts.AddRange(inputPosts.Where(x => !tagPosts.Any(y => y.Id == x.Id)));
                }
                else
                {
                    posts = inputPosts.ToList();
                }
            }
            else
            {
                posts = (await Database.PostsRepository.GetByFilterAsync(true, minDate, onlyFollowings, currentUserId,
                    sortFilter, offset, range)).ToList();
            }

            var postTags = await Database.TagsRepository.GetByPostsAsync(posts.Select(x => x.Id));
            const int minChars = 400;
            foreach (var post in posts)
            {
                if (!post.IsImage)
                {
                    post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
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
            return Mapper.Map<List<TopPostModel>>(posts);
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