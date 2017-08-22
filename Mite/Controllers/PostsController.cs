using System;
using Mite.Helpers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.CodeData.Enums;
using Mite.Models;
using System.Text.RegularExpressions;
using AutoMapper;
using NLog;
using System.Linq;
using Mite.BLL.Infrastructure;
using Mite.BLL.Helpers;

namespace Mite.Controllers
{
    [Authorize]
    public class PostsController : BaseController
    {
        private readonly IServiceBuilder serviceBuilder;
        private readonly ILogger logger;

        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsController(IServiceBuilder serviceBuilder, ILogger logger)
        {
            this.serviceBuilder = serviceBuilder;
            this.logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ShowPost(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid postId))
                return NotFound();

            var postsService = serviceBuilder.Build<IPostsService>();
            var ratingService = serviceBuilder.Build<IRatingService>();

            var post = await postsService.GetWithTagsUserAsync(postId);
            if (post == null || !post.IsPublished)
            {
                return NotFound();
            }
            var userRating = (await ratingService.GetByPostAndUserAsync(postId, User.Identity.GetUserId()))
                ?? new PostRatingModel();
            userRating.PostId = post.Id;

            if (!post.IsImage)
            {
                //Заменяем путь к документу на содержание
                post.Content = await FilesHelper.ReadDocumentAsync(post.Content);
            }
            post.CurrentRating = userRating;
            await postsService.AddViews(postId);
            return View(post);
        }
        public ActionResult AddPost(PostTypes postType)
        {
            ViewBag.Title = "Добавление работы";

            var helpersService = serviceBuilder.Build<IHelpersService>();
            switch (postType)
            {
                case PostTypes.Image:
                    return View("EditImagePost", new ImagePostModel());
                case PostTypes.Document:
                    var helper = helpersService.GetByUser(User.Identity.GetUserId());
                    return View("EditWritePost", new WritingPostModel
                    {
                        Helper = helper
                    });
                default:
                    return NotFound();
            }
        }
        public async Task<ActionResult> EditPost(Guid id)
        {
            ViewBag.Title = "Редактирование работы";

            var postsService = serviceBuilder.Build<IPostsService>();
            var helpersService = serviceBuilder.Build<IHelpersService>();

            var post = await postsService.GetWithTagsAsync(id);
            if (User.Identity.GetUserId() != post.User.Id)
            {
                return Forbidden();
            }
            if (!post.CanEdit)
            {
                return BadRequest();
            }

            if (post.IsImage)
            {
                var imagePost = Mapper.Map<ImagePostModel>(post);
                return View("EditImagePost", imagePost);
            }
            else
            {                
                var writePost = Mapper.Map<WritingPostModel>(post);
                var helper = helpersService.GetByUser(User.Identity.GetUserId());
                //Заменяем путь к документу на содержание
                writePost.Content = await FilesHelper.ReadDocumentAsync(writePost.Content);
                writePost.Helper = helper;
                return View("EditWritePost", writePost);
            }

        }
        [HttpPost]
        public async Task<JsonResult> AddPost(PostModel model)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, "Проверьте правильность заполнения полей");
            if (!string.IsNullOrEmpty(model.Description) && (model.Description.Contains(">") || model.Description.Contains("<")))
                return Json(JsonStatuses.ValidationError, "Обнаружены опасные символы в описании");

            var postsService = serviceBuilder.Build<IPostsService>();

            //Если изображение, сохраняем на сервере, в контент ставим путь к папке
            if (Regex.IsMatch(model.Content, @"<\s*(script|a)") || (!string.IsNullOrEmpty(model.Description) && Regex.IsMatch(model.Description, @"<\s(script)")))
            {
                return Json(JsonStatuses.ValidationError, "Обнаружены опасные данные внутри запроса");
            }
            var result = await postsService.AddPostAsync(model, User.Identity.GetUserId());
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.ValidationError, result.Errors.ToList()[0]);
            }

            return Json(JsonStatuses.Success, "Пост успешно добавлен");
        }
        [HttpPost]
        public async Task<JsonResult> UpdatePost(PostModel model)
        {
            //Здесь не применяется ModelState, потому что Content может быть null
            if (model.Header == null)
            {
                return Json(JsonStatuses.ValidationError, "Заголовок не может быть пустым");
            }
            if (model.Content != null && Regex.IsMatch(model.Content, @"<\s*(script|a)"))
                return Json(JsonStatuses.ValidationError, "Обнаружены опасные данные внутри запроса");

            var postsService = serviceBuilder.Build<IPostsService>();

            var result = await postsService.UpdatePostAsync(model);
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.ValidationError, result.Errors);
            }
            return Json(JsonStatuses.Success, "Успешно отредактировано");
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> DeletePost(Guid id)
        {
            var postsService = serviceBuilder.Build<IPostsService>();

            var post = await postsService.GetPostAsync(id);
            if(User.Identity.GetUserId() != post.User.Id)
            {
                return Forbidden();
            }
            try
            {
                await postsService.DeletePostAsync(id);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error(e, "Ошибка при удалении поста");
                return InternalServerError();
            }
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> PublishPost(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid postId))
            {
                return BadRequest();
            }
            try
            {
                var postsService = serviceBuilder.Build<IPostsService>();

                await postsService.PublishPost(postId);
                return Ok();
            }
            catch (Exception e)
            {
                logger.Error(e, "Ошибка при публикации поста");
                return InternalServerError();
            }
        }
        /// <summary>
        /// Получаем список изображений пользователя (для галереи при показе поста)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> UserGallery(string userId)
        {
            var postsService = serviceBuilder.Build<IPostsService>();

            var result = await postsService.GetGalleryByUserAsync(userId);
            return Json(JsonStatuses.Success, result);
        }
        [AllowAnonymous]
        public async Task<ViewResult> Top()
        {
            var model = new TopModel
            {
                Tags = await serviceBuilder.Build<ITagsService>().GetWithPopularityAsync(true)
            };
            if (User.Identity.IsAuthenticated)
            {
                model.FollowersCount = await serviceBuilder.Build<IFollowersService>().GetFollowersCountAsync(User.Identity.GetUserId());
            }
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Top(PostTopFilterModel filter)
        {
            var postsService = serviceBuilder.Build<IPostsService>();

            //Плюс зарезервированный символ url, поэтому заменяется пробелом
            if(!string.IsNullOrEmpty(filter.Tags))
                filter.Tags = filter.Tags.Replace("18 ", "18+");
            var posts = await postsService.GetTopAsync(filter, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, posts);
        }
    }
}