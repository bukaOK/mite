using System;
using Mite.Helpers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Enums;
using Mite.Models;
using System.Text.RegularExpressions;
using AutoMapper;
using System.Net;
using NLog;
using System.Linq;

namespace Mite.Controllers
{
    [Authorize]
    public class PostsController : BaseController
    {
        private readonly IPostsService postsService;
        private readonly IRatingService ratingService;
        private readonly IHelpersService helpersService;
        private readonly ILogger logger;

        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsController(IPostsService postsService, IRatingService ratingService, IHelpersService helpersService,
            ILogger logger)
        {
            this.postsService = postsService;
            this.ratingService = ratingService;
            this.helpersService = helpersService;
            this.logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ShowPost(string id)
        {
            Guid postId;
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
                return BadRequest();

            var post = await postsService.GetWithTagsUserAsync(postId);
            if (!post.IsPublished)
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
                return JsonResponse(JsonResponseStatuses.ValidationError, "Проверьте правильность заполнения полей");
            if (!string.IsNullOrEmpty(model.Description) && (model.Description.Contains(">") || model.Description.Contains("<")))
                return JsonResponse(JsonResponseStatuses.ValidationError, "Обнаружены опасные символы в описании");
            
            //Если изображение, сохраняем на сервере, в контент ставим путь к папке
            if (Regex.IsMatch(model.Content, @"<\s*(script|a)") || (!string.IsNullOrEmpty(model.Description) && Regex.IsMatch(model.Description, @"<\s(script)")))
            {
                return JsonResponse(JsonResponseStatuses.ValidationError, "Обнаружены опасные данные внутри запроса");
            }
            var result = await postsService.AddPostAsync(model, User.Identity.GetUserId());
            if (!result.Succeeded)
            {
                return JsonResponse(JsonResponseStatuses.ValidationError, result.Errors.ToList()[0]);
            }

            return JsonResponse(JsonResponseStatuses.Success, "Пост успешно добавлен");
        }
        [HttpPost]
        public async Task<JsonResult> UpdatePost(PostModel model)
        {
            //Здесь не применяется ModelState, потому что Content может быть null
            if (model.Header == null)
            {
                return JsonResponse(JsonResponseStatuses.ValidationError, "Заголовок не может быть пустым");
            }
            if (model.Content != null && Regex.IsMatch(model.Content, @"<\s*(script|a)"))
                return JsonResponse(JsonResponseStatuses.ValidationError, "Обнаружены опасные данные внутри запроса");


            var result = await postsService.UpdatePostAsync(model);
            if (!result.Succeeded)
            {
                return JsonResponse(JsonResponseStatuses.ValidationError, result.Errors);
            }
            return JsonResponse(JsonResponseStatuses.Success, "Успешно отредактировано");
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> DeletePost(Guid id)
        {
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
            Guid postId;
            if(string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
            {
                return BadRequest();
            }
            try
            {
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
            var result = await postsService.GetGalleryByUserAsync(userId);
            return JsonResponse(JsonResponseStatuses.Success, result);
        }
        [AllowAnonymous]
        public ViewResult Top()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Top(string tags, string postName, SortFilter sortFilter,
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, int page)
        {
            var tagsNames = string.IsNullOrEmpty(tags) ? new string[0] : tags.Split(',');
            var posts = await postsService.GetTopAsync(tagsNames, postName, sortFilter, postTimeFilter, postUserFilter,
                User.Identity.GetUserId(), page);
            return JsonResponse(JsonResponseStatuses.Success, posts);
        }
    }
}