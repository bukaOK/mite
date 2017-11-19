using Mite.Core;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Mite.ExternalServices.YandexMoney;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.BLL.Services;

namespace Mite.Controllers
{
    public class YandexController : BaseController
    {
        private readonly IYandexMoneyService yandexService;
        private readonly IPaymentService paymentService;

        public YandexController(IYandexMoneyService yandexService, IPaymentService paymentService)
        {
            this.yandexService = yandexService;
            this.paymentService = paymentService;
        }
        public async Task<ActionResult> Authorize(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                await yandexService.AuthorizeAsync(User.Identity.GetUserId(), code);
            }
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Cash");
            }
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Сюда перенаправляет яндекс.деньги после успешного заполнения данных банковской карты
        /// </summary>
        /// <param name="success"></param>
        /// <returns></returns>
        public ActionResult ConfirmExternalPayment(bool? success)
        {
            var sessionPayment = Session[SessionKeys.YaMoneyExternal];
            if(sessionPayment == null)
            {
                success = null;
            }
            return View(success);
        }
        [HttpPost]
        public async Task<ActionResult> ConfirmExternalPayment()
        {
            var sessionPayment = (ExternalPayment)Session[SessionKeys.YaMoneyExternal];

            var result = await yandexService.ExternalPayIn(sessionPayment.RequestID, sessionPayment.InstanceID);
            if (result.Succeeded)
            {
                await paymentService.AddAsync(sessionPayment.Sum, sessionPayment.RequestID, User.Identity.GetUserId(), PaymentType.BankCard);
                Session[SessionKeys.YaMoneyExternal] = null;
                return Json(JsonStatuses.Success);
            }
            else
            {
                Session[SessionKeys.YaMoneyExternal] = null;
                return Json(JsonStatuses.ValidationError, result.Errors);
            }
        }
    }
}