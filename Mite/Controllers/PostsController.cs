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
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mite.Controllers
{
    [Authorize]
    public class PostsController : BaseController
    {
        private readonly IPostsService _postsService;
        private readonly IRatingService _ratingService;

        public PostsController(IPostsService postsService, IRatingService ratingService)
        {
            _postsService = postsService;
            _ratingService = ratingService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ShowPost(string id)
        {
            Guid postId;
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var post = await _postsService.GetWithTagsUserAsync(postId);
            if (!post.IsPublished)
            {
                return new HttpNotFoundResult();
            }
            var userRating = (await _ratingService.GetByPostAndUserAsync(postId, User.Identity.GetUserId()))
                ?? new PostRatingModel();
            userRating.PostId = post.Id;
            
            if (!post.IsImage)
            {
                //Заменяем путь к документу на содержание
                post.Content = await FilesHelper.ReadDocumentAsync(post.Content);
            }
            post.CurrentRating = userRating;
            await _postsService.AddViews(postId);
            return View(post);
        }
        public ActionResult AddPost(PostTypes postType)
        {
            ViewBag.Title = "Добавление работы";

            if (postType == PostTypes.Image)
            {
                return View("EditImagePost", new ImagePostModel());
            }
            if (postType == PostTypes.Document)
            {
                return View("EditWritePost", new WritingPostModel());
            }
            return new HttpNotFoundResult();
        }
        public async Task<ActionResult> EditPost(string id)
        {
            Guid postId;
            ViewBag.Title = "Редактирование работы";

            if(string.IsNullOrEmpty(id) || !Guid.TryParse(id, out postId))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            try
            {
                var post = await _postsService.GetWithTagsAsync(postId);
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
                    //Заменяем путь к документу на содержание
                    writePost.Content = await FilesHelper.ReadDocumentAsync(writePost.Content);
                    return View("EditWritePost", writePost);
                }
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> AddPost(PostModel model)
        {
            string postsFolder;
            if (!ModelState.IsValid)
                return JsonResponse(JsonResponseStatuses.ValidationError, GetErrorsList());
            if (model.IsImage)
            {
                postsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
            }
            else
            {
                postsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";
            }
            if (Regex.IsMatch(model.Content, @"<\s*(script|a|img)"))
            {
                return JsonResponse(JsonResponseStatuses.Error, "Обнаружены опасные данные внутри запроса");
            }
            await _postsService.AddPostAsync(model, postsFolder, User.Identity.GetUserId());
            return JsonResponse(JsonResponseStatuses.Success, "Пост успешно добавлен");
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<JsonResult> UpdatePost(PostModel model)
        {
            //Здесь не применяется ModelState, потому что Content может быть null
            if (model.Header == null)
            {
                ModelState.AddModelError("", "Заголовок не может быть пустым");
                return JsonResponse(JsonResponseStatuses.Error, GetErrorsList());
            }
            if (model.Content != null && Regex.IsMatch(model.Content, @"<\s*(script|a|img)"))
                return JsonResponse(JsonResponseStatuses.Error, "Обнаружены опасные данные внутри запроса");

            var postsFolder = model.IsImage 
                ? HostingEnvironment.ApplicationVirtualPath + "Public/images/"
                : HostingEnvironment.ApplicationVirtualPath + "Public/documents/";
            await _postsService.UpdatePostAsync(model, postsFolder);
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
                await _postsService.DeletePostAsync(postId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch(Exception)
            {
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
                await _postsService.PublishPost(postId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }
        public async Task<JsonResult> UserPosts(string userId, SortFilter sort, PostTypes? type)
        {
            IEnumerable<ProfilePostModel> posts = new List<ProfilePostModel>();
            if (type == null || type == PostTypes.Published)
            {
                posts = await _postsService.GetByUserAsync(userId, sort, PostTypes.Published);
            }
            else if(User.Identity.GetUserId() == userId)
            {
                posts = await _postsService.GetByUserAsync(userId, sort, PostTypes.Drafts);
            }
            return JsonResponse(JsonResponseStatuses.Success, posts);
        }
        public ViewResult Top()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> Top(string input, SortFilter sortFilter,
            PostTimeFilter postTimeFilter, PostUserFilter postUserFilter, int page)
        {
            var posts = await _postsService.GetTopAsync(input, sortFilter, postTimeFilter, postUserFilter,
                User.Identity.GetUserId(), page);
            return JsonResponse(JsonResponseStatuses.Success, posts);
        }
    }
}