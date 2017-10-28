using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class ClientProfileController : BaseController
    {
        private readonly IUserService userService;
        private readonly IFollowersService followersService;

        public ClientProfileController(IUserService userService, IFollowersService followersService)
        {
            this.userService = userService;
            this.followersService = followersService;
        }
        public async Task<ActionResult> Index(string name)
        {
            var profile = await userService.GetClientProfileAsync(name);
            if (profile == null)
                return BadRequest();

            return View("Index", profile);
        }
        public Task<ActionResult> Followings(string name)
        {
            return Index(name);
        }
        [HttpPost]
        public async Task<JsonResult> Followings(string name, SortFilter sort)
        {
            var followings = await followersService.GetFollowingsByUserAsync(name, sort);

            const byte maxAboutLength = 70;
            const byte maxNameLength = 13;
            foreach (var fol in followings)
            {
                if (fol.Description.Length > maxAboutLength)
                    fol.Description = fol.Description.Substring(0, maxAboutLength - 3) + "...";
                if (fol.UserName.Length > maxNameLength)
                    fol.UserName = fol.UserName.Substring(0, maxNameLength - 3) + "...";
            }
            return Json(JsonStatuses.Success, followings);
        }
    }
}