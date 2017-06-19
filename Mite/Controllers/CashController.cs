using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Constants;
using Mite.Core;
using Mite.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class CashController : BaseController
    {
        private readonly AppUserManager userManager;
        private readonly ICashService cashService;
        /// <summary>
        /// Общая комиссия за все платежи(вывод денег) 2%.
        /// </summary>
        private const double Comission = 0.02;

        public CashController(AppUserManager userManager, ICashService cashService)
        {
            this.userManager = userManager;
            this.cashService = cashService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="epr">External payment result(результат ответа от банка)</param>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var cash = await cashService.GetUserCashAsync(User.Identity.GetUserId());
            var model = new CashModel
            {
                CashSum = cash,
                IsYandexAuthorized = await cashService.IsYandexAuthorized(User.Identity.GetUserId()),
                ClientId = YaMoneySettings.ClientId,
                RedirectUri = YaMoneySettings.RedirectUri,
                SystemYandexWallet = YaMoneySettings.WalletId
            };
            return View(model);
        }
        public PartialViewResult Wallets()
        {
            var user = userManager.FindById(User.Identity.GetUserId());
            var model = new WalletsSettingsModel
            {
                YandexWalId = user.YandexWalId
            };
            return PartialView("Wallets", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateWallets(WalletsSettingsModel model)
        {
            var user = await userManager.FindByIdAsync(User.Identity.GetUserId());
            var isPasswordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                return JsonResponse(JsonResponseStatuses.ValidationError, "Неверный пароль");

            user.YandexWalId = model.YandexWalId;
            await userManager.UpdateAsync(user);
            return JsonResponse(JsonResponseStatuses.Success, "Яндекс кошелек успешно добавлен/обновлен");
        }
        [HttpPost]
        public async Task<JsonResult> PaymentsHistory()
        {
            var payments = await cashService.GetPaymentsHistoryAsync(User.Identity.GetUserId());
            return JsonResponse(JsonResponseStatuses.Success, payments);
        }
        [HttpPost]
        public async Task<JsonResult> Referals()
        {
            var referals = await cashService.GetReferalsByUserAsync(User.Identity.GetUserId());
            return JsonResponse(JsonResponseStatuses.Success, referals);
        }
    }
}