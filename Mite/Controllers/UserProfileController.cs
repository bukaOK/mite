using System.Threading.Tasks;
using System.Web.Mvc;
using Mite.BLL.Services;
using Mite.Core;
using System.Net;
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
            return View(profile);
        }
        public ViewResult Notifications()
        {
            return View();
        }
        public async Task<JsonResult> Followers(string userId, SortFilter sort)
        {
            var userModels = await _followersService.GetFollowersByUserAsync(userId);

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
            return JsonResponse(JsonResponseStatuses.Success, userModels);
        }
        public async Task<JsonResult> Posts(string userId, SortFilter sort, PostTypes? type)
        {
            IEnumerable<ProfilePostModel> posts;
            if (userId != User.Identity.GetUserId() && type == PostTypes.Drafts)
                return null;
            if (type == null)
                type = PostTypes.Published;
            posts = await _postsService.GetByUserAsync(userId, sort, (PostTypes)type);
            return JsonResponse(JsonResponseStatuses.Success, posts);
        }
        public async Task<JsonResult> Followings(string userId, SortFilter sort)
        {
            var userModels = await _followersService.GetFollowingsByUserAsync(userId);

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

            return JsonResponse(JsonResponseStatuses.Success, userModels);
        }
    }
}