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

namespace Mite.BLL.Services
{
    public interface IPostsService
    {
        Task<PostModel> GetPostAsync(Guid postId);
        Task AddPostAsync(PostModel postModel, string postsFolder, string userId);
        Task DeletePostAsync(Guid postId);
        Task UpdatePostAsync(PostModel postModel, string postsFolder);
        Task<PostModel> GetWithTagsAsync(Guid postId);
        /// <summary>
        /// Возвращает вместе с тегами и владельцем поста
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<PostModel> GetWithTagsUserAsync(Guid postId);
        Task<IEnumerable<ProfilePostModel>> GetByUserAsync(string userId, SortFilter sort, PostTypes type);
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

        public async Task AddPostAsync(PostModel postModel, string postsFolder, string userId)
        {
            var filePath = postModel.IsImage
                ? FilesHelper.CreateImage(postsFolder, postModel.Content)
                : FilesHelper.CreateDocument(postsFolder, postModel.Content);

            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var post = Mapper.Map<Post>(postModel);

            post.Id = Guid.NewGuid();
            post.Content = filePath;
            post.UserId = userId;
            post.LastEdit = DateTime.UtcNow;
            post.Rating = 0;
            post.IsPublished = false;

            await Database.PostsRepository.AddAsync(post);
            foreach (var tag in post.Tags)
                tag.Name = tag.Name.ToLower();
            await Database.TagsRepository.AddWithPostAsync(post.Tags, post.Id);
        }

        public async Task DeletePostAsync(Guid postId)
        {
            var post = await Database.PostsRepository.GetAsync(postId);
            FilesHelper.DeleteFile(post.Content);
            await Database.PostsRepository.RemoveAsync(postId);
        }
        /// <summary>
        /// Обновляет пост, PostModel.Content может быть null(когда не было изменений), если это файл
        /// </summary>
        /// <param name="postModel">модель</param>
        /// <param name="postsFolder">папка для сохранения поста</param>
        /// <returns></returns>
        public async Task UpdatePostAsync(PostModel postModel, string postsFolder)
        {
            var currentPost = await Database.PostsRepository.GetAsync(postModel.Id);
            postModel.Tags = postModel.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var post = Mapper.Map<Post>(postModel);

            if (postModel.IsImage && postModel.Content != currentPost.Content)
            {
                FilesHelper.DeleteFile(currentPost.Content);
                post.Content = FilesHelper.CreateImage(postsFolder, postModel.Content) ;
            }
            else if(!postModel.IsImage)
            {
                if(postModel.Content != null)
                    FilesHelper.UpdateDocument(currentPost.Content, postModel.Content);
                post.Content = currentPost.Content;
            }
            post.LastEdit = DateTime.UtcNow;

            foreach (var tag in post.Tags)
                tag.Name = tag.Name.ToLower();
            await Database.TagsRepository.AddWithPostAsync(post.Tags, post.Id);
            await Database.PostsRepository.UpdateAsync(post);
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
            foreach(var post in posts)
            {
                if (!post.IsImage)
                {
                    const int minChars = 400;
                    var charsCount = FilesHelper.GetDocCharsCount(post.Content);
                    post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
                    if (charsCount > minChars)
                        post.Content += "...";
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
            const int range = 20;
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
                var tags = await Database.TagsRepository.GetByNameAsync(input);
                //Находим 
                var inputPosts = await Database.PostsRepository.GetByNameAsync(input, true, minDate, onlyFollowings, currentUserId,
                        sortFilter, offset, range);
                if (tags.Count() > 0)
                {
                    var tagPosts = await Database.PostsRepository.GetByTagsAsync(tags.Select(x => x.Id), true, minDate, onlyFollowings,
                        currentUserId, sortFilter, offset, range);
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
            foreach (var post in posts)
            {
                if (!post.IsImage)
                {
                    const int minChars = 400;
                    var charsCount = FilesHelper.GetDocCharsCount(post.Content);
                    post.Content = await FilesHelper.ReadDocumentAsync(post.Content, minChars);
                    if (charsCount > minChars)
                        post.Content += "...";
                }
            }
            return Mapper.Map<List<TopPostModel>>(posts);
        }
    }
}