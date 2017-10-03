using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Helpers;
using Mite.Infrastructure;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class CitiesController : BaseController
    {
        private readonly ICityService cityService;
        private readonly MiteCookie miteCookie;
        private readonly AppUserManager userManager;

        public CitiesController(ICityService cityService, MiteCookie miteCookie, AppUserManager userManager)
        {
            this.miteCookie = miteCookie;
            this.userManager = userManager;
            this.cityService = cityService;
        }
        [HttpPost]
        public async Task<JsonResult> BindCity(string cityName)
        {
            var city = await cityService.GetByNameAsync(cityName);
            if(city != null)
            {
                string cityId;
                if (User.Identity.IsAuthenticated)
                {
                    var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
                    cityId = user.CityId?.ToString();
                    if (string.IsNullOrEmpty(cityId))
                    {
                        var result = await cityService.AddCityToUserAsync(city.Id, User.Identity.GetUserId());
                        cityId = city.Id.ToString();

                        if (!result.Succeeded)
                            return Json(JsonStatuses.Error);
                    }
                }
                else
                {
                    cityId = miteCookie[CookieKeys.UserCityId];
                    if (string.IsNullOrEmpty(cityId))
                    {
                        cityId = city.Id.ToString();
                        miteCookie[CookieKeys.UserCityId] = cityId;
                    }
                }
                return Json(JsonStatuses.Success);
            }
            return Json(JsonStatuses.Error);
        }
        /// <summary>
        /// Привязать город по координатам
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //public Task<JsonResult> BindCityByIp()
        //{
        //    var ip = Request.UserHostAddress;
        //    if(ip == "127.0.0.1")
        //        ip = ""
        //}
    }
}