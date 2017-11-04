﻿using System;
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
using System.Collections.Generic;
using Mite.CodeData.Constants;

namespace Mite.Controllers
{
    [Authorize(Roles = RoleNames.Author)]
    public class PostsController : BaseController
    {
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly string documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";

        private readonly IPostsService postsService;
        private readonly IRatingService ratingService;
        private readonly IHelpersService helpersService;
        private readonly ITagsService tagsService;
        private readonly IFollowersService followersService;

        public PostsController(IPostsService postsService, IRatingService ratingService, IHelpersService helpersService, 
            ITagsService tagsService, IFollowersService followersService)
        {
            this.postsService = postsService;
            this.ratingService = ratingService;
            this.helpersService = helpersService;
            this.tagsService = tagsService;
            this.followersService = followersService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ShowPost(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid postId))
                return NotFound();

            var post = await postsService.GetWithTagsUserAsync(postId);
            if (post == null || post.Type != PostTypes.Published)
            {
                return NotFound();
            }
            var userRating = (await ratingService.GetByPostAndUserAsync(postId, User.Identity.GetUserId()))
                ?? new PostRatingModel();
            userRating.PostId = post.Id;

            post.CurrentRating = userRating;
            await postsService.AddViews(postId);
            return View(post);
        }
        public async Task<ActionResult> AddPost(PostContentTypes postType)
        {
            ViewBag.Title = "Добавление работы";
            var tags = (await tagsService.GetForUserAsync()).ToList();
            
            switch (postType)
            {
                case PostContentTypes.Image:
                    return View("EditImagePost", new ImagePostModel
                    {
                        Tags = tags
                    });
                case PostContentTypes.Document:
                    return View("EditWritePost", new WritingPostModel
                    {
                        Helper = helpersService.GetByUser(User.Identity.GetUserId()),
                        Tags = tags
                    });
                case PostContentTypes.ImageCollection:
                    return View("EditImageCollection", new ImagePostModel
                    {
                        Tags = tags
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
            switch (post.ContentType)
            {
                case PostContentTypes.Image:
                    var imagePost = Mapper.Map<ImagePostModel>(post);
                    return View("EditImagePost", imagePost);
                case PostContentTypes.ImageCollection:
                    var imageCollection = Mapper.Map<ImagePostModel>(post);
                    return View("EditImageCollection", imageCollection);
                case PostContentTypes.Document:
                    var writePost = Mapper.Map<WritingPostModel>(post);
                    //Заменяем путь к документу на содержание
                    writePost.Content = await FilesHelper.ReadDocumentAsync(writePost.Content);
                    writePost.Helper = await helpersService.GetByUserAsync(User.Identity.GetUserId());
                    return View("EditWritePost", writePost);
                default:
                    throw new NotImplementedException("Неизвестный тип контента работы");
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddPost(PostModel model)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, "Проверьте правильность заполнения полей");
            if (!string.IsNullOrEmpty(model.Description) && Regex.IsMatch(model.Description, @"<\s*(a|script)\s+"))
                return Json(JsonStatuses.ValidationError, "Обнаружены опасные символы в описании");

            if (model.PublishDate != null)
                model.Type = PostTypes.Published;
            else
                model.Type = PostTypes.Drafts;

            //Если изображение, сохраняем на сервере, в контент ставим путь к папке
            if (!string.IsNullOrEmpty(model.Content) && Regex.IsMatch(model.Content, @"<\s*(a|script)\s+"))
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

            var result = await postsService.UpdatePostAsync(model);
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.ValidationError, result.Errors.ToArray()[0]);
            }
            return Json(JsonStatuses.Success, "Успешно отредактировано");
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> DeletePost(Guid id)
        {
            var post = await postsService.GetPostAsync(id);
            if(User.Identity.GetUserId() != post.User.Id)
            {
                return Forbidden();
            }
            var result = await postsService.DeletePostAsync(id);
            if(result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> PublishPost(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid postId))
            {
                return BadRequest();
            }
            var result = await postsService.PublishPost(postId);
            if(result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        /// <summary>
        /// Получаем список изображений пользователя (для галереи при показе поста)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<JsonResult> UserGallery(string userId, Guid postId)
        {
            var result = await postsService.GetGalleryByUserAsync(userId);
            result.InitialIndex = Array.IndexOf(result.Items, result.Items.First(x => string.Equals(x.Id, postId.ToString())));

            return Json(JsonStatuses.Success, result, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public async Task<ViewResult> Top()
        {
            var model = new TopModel
            {
                Tags = await tagsService.GetWithPopularityAsync(true)
            };
            if (User.Identity.IsAuthenticated)
            {
                model.FollowersCount = await followersService.GetFollowersCountAsync(User.Identity.GetUserId());
            }
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Top(PostTopFilterModel filter)
        {
            //Плюс зарезервированный символ url, поэтому заменяется пробелом
            if(!string.IsNullOrEmpty(filter.Tags))
                filter.Tags = filter.Tags.Replace("18 ", "18+");
            var posts = await postsService.GetTopAsync(filter, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, posts);
        }
    }
}