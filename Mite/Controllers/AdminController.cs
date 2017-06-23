using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.Core;
using Mite.ExternalServices.YandexMoney;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Mite.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        private readonly AppUserManager userManager;
        private readonly IYandexService yandexService;
        private readonly AppRoleManager roleManager;

        public AdminController(AppUserManager userManager, IYandexService yandexService, AppRoleManager roleManager)
        {
            this.userManager = userManager;
            this.yandexService = yandexService;
            this.roleManager = roleManager;
        }
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult ModerRegister()
        {
            var role = roleManager.FindByName("moder");
            var users = userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
            return PartialView(users);
        }
        [HttpPost]
        public async Task<ActionResult> AddModer(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                return JsonResponse(JsonResponseStatuses.ValidationError, "Заполни поле с именем модератора");
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                return JsonResponse(JsonResponseStatuses.ValidationError, "Не найден пользователь с таким именем.");
            await userManager.AddToRoleAsync(user.Id, "moder");
            return JsonResponse(JsonResponseStatuses.Success, null);
        }
    }
}