using System.Threading.Tasks;
using System.Web.Mvc;
using Mite.BLL.Services;
using Mite.Core;
using Microsoft.AspNet.Identity;
using Mite.Enums;
using System.Linq;
using System;
using System.Collections.Generic;
using Mite.Models;

namespace Mite.Controllers
{
    public class UserProfileController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IPostsService _postsService;
        private readonly IFollowersService _followersService;

        public UserProfileController(IUserService userService, IPostsService postsService, IFollowersService followersService)
        {
            _userService = userService;
            _postsService = postsService;
            _followersService = followersService;
        }
        /// <summary>
        /// Страница пользователя
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Index(string name)
        {
            var profile = await _userService.GetUserProfileAsync(name ?? User.Identity.Name, User.Identity.GetUserId());

            if (profile == null)
                return NotFound();

            profile.SocialLinks = await _userService.GetSocialLinksAsync(profile.UserId.ToString());
            return View("Index", profile);
        }
        public async Task<JsonResult> GetUserProfile(string name)
        {
            name = name.Replace(" ", string.Empty);
            var profile = await _userService.GetUserProfileAsync(name, User.Identity.GetUserId());
            profile.SocialLinks = await _userService.GetSocialLinksAsync(profile.UserId.ToString());
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
            return Json(JsonStatuses.Success, profile, JsonRequestBehavior.AllowGet);
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
        public async Task<JsonResult> Followers(string name, SortFilter sort)
        {
            var userModels = await _followersService.GetFollowersByUserAsync(name);

            switch (sort)
            {
                case SortFilter.New:
                    userModels = userModels.OrderByDescending(x => x.RegisterDate)
                        .ThenByDescending(x => x.Rating).ToList();
                    break;
                case SortFilter.Old:
                    userModels = userModels.OrderBy(x => x.RegisterDate)
                        .ThenByDescending(x => x.Rating).ToList();
                    break;
                case SortFilter.Popular:
                    userModels = userModels.OrderByDescending(x => x.Rating)
                        .ThenBy(x => x.RegisterDate).ToList();
                    break;
                default:
                    throw new NotImplementedException("Неизвестный тип сортировки");
            }
            return Json(JsonStatuses.Success, userModels, JsonRequestBehavior.AllowGet);
        }
        public Task<ActionResult> Posts(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<ActionResult> Posts(string name, SortFilter sort, PostTypes? type, int page)
        {
            IEnumerable<ProfilePostModel> posts;
            if (!string.Equals(name, User.Identity.Name, StringComparison.OrdinalIgnoreCase) && type == PostTypes.Drafts)
                return Forbidden();
            if (type == null)
                type = PostTypes.Published;
            posts = await _postsService.GetByUserAsync(name, User.Identity.GetUserId(), sort, (PostTypes)type, page);
            return Json(JsonStatuses.Success, posts);
        }
        public Task<ActionResult> Followings(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<JsonResult> Followings(string name, SortFilter sort)
        {
            var userModels = await _followersService.GetFollowingsByUserAsync(name);

            switch (sort)
            {
                case SortFilter.New:
                    userModels = userModels.OrderByDescending(x => x.RegisterDate)
                        .ThenByDescending(x => x.Rating).ToList();
                    break;
                case SortFilter.Old:
                    userModels = userModels.OrderBy(x => x.RegisterDate)
                        .ThenByDescending(x => x.Rating).ToList();
                    break;
                case SortFilter.Popular:
                    userModels = userModels.OrderByDescending(x => x.Rating)
                        .ThenBy(x => x.RegisterDate).ToList();
                    break;
                default:
                    throw new NotImplementedException("Неизвестный тип сортировки");
            }

            return Json(JsonStatuses.Success, userModels, JsonRequestBehavior.AllowGet);
        }
    }
}