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
using System.Collections.Generic;
using System.Web;
using Mite.Infrastructure;

namespace Mite.Controllers
{
    [Authorize]
    public class PostsController : BaseController
    {
        private readonly IPostsService postsService;
        private readonly IRatingService ratingService;
        private readonly IHelpersService helpersService;
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        public PostsController(IPostsService postsService, IRatingService ratingService, IHelpersService helpersService)
        {
            this.postsService = postsService;
            this.ratingService = ratingService;
            this.helpersService = helpersService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ShowPost(string id)
        {
            Guid postId;
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var post = await postsService.GetWithTagsUserAsync(postId);
            if (!post.IsPublished)
            {
                return new HttpNotFoundResult();
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
                    return new HttpNotFoundResult();
            }
        }
        public async Task<ActionResult> EditPost(string id)
        {
            Guid postId;
            ViewBag.Title = "Редактирование работы";

            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var post = await postsService.GetWithTagsAsync(postId);
            if (User.Identity.GetUserId() != post.User.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (post.IsPublished)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                return JsonResponse(JsonResponseStatuses.ValidationError, result.Errors);
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
        public async Task<HttpStatusCodeResult> DeletePost(string id)
        {
            Guid postId;
            if (!Guid.TryParse(id, out postId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                await postsService.DeletePostAsync(postId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                Logger.WriteError(e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> PublishPost(string id)
        {
            Guid postId;
            if(string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                await postsService.PublishPost(postId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
        public async Task<JsonResult> UserPosts(string userId, SortFilter sort, PostTypes? type)
        {
            IEnumerable<ProfilePostModel> posts = new List<ProfilePostModel>();
            if (type == null || type == PostTypes.Published)
            {
                posts = await postsService.GetByUserAsync(userId, sort, PostTypes.Published);
            }
            else if(User.Identity.GetUserId() == userId)
            {
                posts = await postsService.GetByUserAsync(userId, sort, PostTypes.Drafts);
            }
            return JsonResponse(JsonResponseStatuses.Success, posts);
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
        public ViewResult Top()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> Top(string input, SortFilter sortFilter,
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, int page)
        {
            var posts = await postsService.GetTopAsync(input, sortFilter, postTimeFilter, postUserFilter,
                User.Identity.GetUserId(), page);
            return JsonResponse(JsonResponseStatuses.Success, posts);
        }
    }
}