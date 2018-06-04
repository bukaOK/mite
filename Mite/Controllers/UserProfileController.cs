using System.Threading.Tasks;
using System.Web.Mvc;
using Mite.BLL.Services;
using Mite.Core;
using Microsoft.AspNet.Identity;
using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using Mite.Models;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Infrastructure;

namespace Mite.Controllers
{
    public class UserProfileController : BaseController
    {
        private readonly IUserService userService;
        private readonly IPostsService postsService;
        private readonly IFollowersService followersService;
        private readonly IAuthorServiceService authorService;
        private readonly IProductsService productsService;

        public UserProfileController(IUserService userService, IPostsService postsService, IFollowersService followersService,
            IAuthorServiceService authorService, IProductsService productsService, AppUserManager userManager)
        {
            this.userService = userService;
            this.postsService = postsService;
            this.followersService = followersService;
            this.authorService = authorService;
            this.productsService = productsService;
        }
        /// <summary>
        /// Страница пользователя
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Index(string name)
        {
            var profile = await userService.GetUserProfileAsync(name, User.Identity.GetUserId());
            if (profile == null)
                return NotFound();

            return profile.IsAuthor ? View("Index", profile) : View("ClientIndex", profile);
        }
        /// <summary>
        /// Для мини-карточки пользователя
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetUserProfile(string name)
        {
            name = name.Replace(" ", string.Empty);
            var profile = await userService.GetUserProfileAsync(name, User.Identity.GetUserId());
            if (profile == null)
                return NotFound();
            //Обрезаем описание(всё для красоты)
            if(!string.IsNullOrEmpty(profile.About) && profile.About.Length > 50)
            {
                var about = profile.About;
                //Находим последний пробел, обрезаем до него
                about = about.Substring(0, 50);
                var lastSpace = about.LastIndexOf(' ');
                about = about.Substring(0, lastSpace);
                //Убираем последний символ, если это знак препинания
                var lastChar = about[about.Length - 1];
                if (lastChar == ',' || lastChar == '.' || lastChar == '-')
                {
                    about = about.Substring(0, about.Length - 1);
                }
                profile.About = about + "...";
            }
            return Json(JsonStatuses.Success, profile);
        }
        public ViewResult Notifications()
        {
            return View();
        }
        public Task<ActionResult> Followers(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<ActionResult> Followers(string name, SortFilter sort)
        {
            var followers = await followersService.GetFollowersByUserAsync(name, sort);
            return Json(JsonStatuses.Success, followers);
        }
        public Task<ActionResult> Posts(string name)
        {
            return Index(name);
        }
        [HttpGet]
        public Task<ActionResult> Services(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<ActionResult> Posts(string name, SortFilter sort, PostTypes? type)
        {
            IEnumerable<ProfilePostModel> posts;
            if (!string.Equals(name, User.Identity.Name, StringComparison.OrdinalIgnoreCase) && type == PostTypes.Drafts)
                return Forbidden();
            if (type == null)
                type = PostTypes.Published;
            posts = await postsService.GetByUserAsync(name, User.Identity.GetUserId(), sort, (PostTypes)type);
            return Json(JsonStatuses.Success, posts);
        }
        public Task<ActionResult> Followings(string name)
        {
            return Index(name);
        }
        public Task<ActionResult> Favorites(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<ActionResult> Followings(string name, SortFilter sort)
        {
            var followings = await followersService.GetFollowingsByUserAsync(name, sort);
            return Json(JsonStatuses.Success, followings);
        }
        [HttpPost]
        public async Task<ActionResult> Services(string name, SortFilter sort)
        {
            try
            {
                var services = await authorService.GetByUserAsync(name, sort);
                return Json(JsonStatuses.Success, services);
            }
            catch (DataServiceException e)
            {
                return Json(JsonStatuses.ValidationError, e.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Favorites(string name, SortFilter sort)
        {
            try
            {
                var posts = await postsService.GetByUserAsync(name, CurrentUserId, sort, PostTypes.Favorite);
                return Json(JsonStatuses.Success, posts);
            }
            catch (DataServiceException e)
            {
                return Json(JsonStatuses.ValidationError, e.Message);
            }
        }
        [HttpGet]
        public Task<ActionResult> Products(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<ActionResult> Products(string name, SortFilter sort)
        {
            try
            {
                var products = await productsService.GetForUserAsync(name, sort, CurrentUserId);
                return Json(JsonStatuses.Success, products);
            }
            catch(Exception e)
            {
                return Json(JsonStatuses.ValidationError, e.Message);
            }
        }
    }
}