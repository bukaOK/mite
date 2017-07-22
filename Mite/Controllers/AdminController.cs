using Hangfire;
using Microsoft.AspNet.Identity;
using Mite.Attributes.Filters;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Core;
using Mite.DAL.Infrastructure;
using Mite.ExternalServices.YandexMoney;
using Mite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Mite.Controllers
{
    [Authorize(Roles = "admin")]
    [AjaxOnly("Index")]
    public class AdminController : BaseController
    {
        private readonly AppUserManager userManager;
        private readonly IYandexService yandexService;
        private readonly AppRoleManager roleManager;
        private readonly IUnitOfWork unitOfWork;

        public AdminController(AppUserManager userManager, IYandexService yandexService, AppRoleManager roleManager, 
            IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.yandexService = yandexService;
            this.roleManager = roleManager;
            this.unitOfWork = unitOfWork;
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
        public PartialViewResult Statistic()
        {
            var model = new AdminStatisticModel();
            model.UsersCount = userManager.Users.Count();
            model.PostsCount = unitOfWork.PostsRepository.GetCount();
            return PartialView(model);
        }
    }
}