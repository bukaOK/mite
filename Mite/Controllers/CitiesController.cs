using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Extensions;
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
        private readonly AppUserManager userManager;
        private readonly IAuthenticationManager authenticationManager;

        public CitiesController(ICityService cityService, AppUserManager userManager,
            IAuthenticationManager authenticationManager)
        {
            this.userManager = userManager;
            this.authenticationManager = authenticationManager;
            this.cityService = cityService;
        }
        [HttpPost]
        public async Task<ActionResult> BindCity(string cityName)
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
                    User.Identity.AddUpdateClaim(authenticationManager, ClaimConstants.UserCityId, city.Id.ToString());
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