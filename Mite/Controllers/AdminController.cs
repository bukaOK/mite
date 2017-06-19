using Mite.BLL.IdentityManagers;
using Mite.Core;
using Mite.ExternalServices.YandexMoney;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        private readonly AppUserManager _userManager;
        private readonly IYandexService yandexService;

        public AdminController(AppUserManager userManager, IYandexService yandexService)
        {
            _userManager = userManager;
            this.yandexService = yandexService;
        }
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult ModerRegister()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<ActionResult> AddModer(string userName)
        {
            if(string.IsNullOrEmpty(userName))
                return JsonResponse(JsonResponseStatuses.ValidationError, "Заполни поле с именем модератора");
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return JsonResponse(JsonResponseStatuses.ValidationError, "Не найден пользователь с таким именем.");
            await _userManager.AddToRoleAsync(user.Id, "moder");
            return JsonResponse(JsonResponseStatuses.Success, null);
        }
    }
}