using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.CodeData.Enums;
using Mite.ExternalServices.YandexMoney;
using Mite.Models;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Yandex.Money.Api.Sdk.Exceptions;
using Yandex.Money.Api.Sdk.Responses;
using Mite.BLL.Infrastructure;

namespace Mite.Controllers
{
    [Authorize]
    public class PaymentsController : BaseController
    {
        private const double Comission = 0.02;

        private readonly AppUserManager userManager;
        private readonly ILogger logger;
        private readonly IYandexMoneyService yaService;
        private readonly IPaymentService paymentService;
        private readonly IExternalServices externalServices;
        private readonly ICashService cashService;
        private readonly IWebMoneyService wmService;

        public PaymentsController(AppUserManager userManager, ILogger logger, IYandexMoneyService yaService, 
            IPaymentService paymentService, IExternalServices externalServices, ICashService cashService, IWebMoneyService wmService)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.yaService = yaService;
            this.paymentService = paymentService;
            this.externalServices = externalServices;
            this.cashService = cashService;
            this.wmService = wmService;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PayInYandex(YaPayInModel model)
        {
            var sum = (double)model.YaPayInSum;
            var userId = User.Identity.GetUserId();
            try
            {
                var result = await yaService.PayInAsync(sum, userId);
                if (result.Succeeded)
                {
                    //Сохраняем операцию в базу
                    var operationId = (string)result.ResultData;
                    await paymentService.AddAsync(sum, operationId, userId, PaymentType.YandexWallet);
                    return Json(JsonStatuses.Success);
                }
                else
                {
                    return Json(JsonStatuses.ValidationError, result.Errors);
                }
            }
            catch (InvalidTokenException e)
            {
                var userLogins = await userManager.GetLoginsAsync(userId);
                var yaLogin = userLogins.FirstOrDefault(x => x.LoginProvider == YaMoneySettings.DefaultAuthType);
                logger.Error(e, "Истек срок Яндекс токена.");
                await externalServices.RemoveAsync(userId, YaMoneySettings.DefaultAuthType);
                return Json(JsonStatuses.ValidationError, new string[] { "Истек срок токена, перезагрузите страницу и попробуйте снова авторизовать приложение" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PayOutYandex(YaPayOutModel model)
        {
            if (model.PayOutSum == 0 || model.PayOutSum == null)
            {
                return Json(JsonStatuses.ValidationError, new[] { "Сумма не может быть 0." });
            }
            if(model.PayOutSum < 500 && !User.IsInRole(RoleNames.Moderator))
            {
                return Json(JsonStatuses.ValidationError, new[] { "Минимальная сумма для вывода - 500 руб." });
            }
            var sum = (double)model.PayOutSum;
            var userCash = await cashService.GetUserCashAsync(User.Identity.GetUserId());

            if (sum > userCash)
            {
                return Json(JsonStatuses.ValidationError, new [] { "Требуемая сумма больше вашего баланса." });
            }
            var user = await userManager.FindByIdAsync(User.Identity.GetUserId());

            //Сумма комиссии
            var comissionSum = sum * Comission;
            //Итоговая сумма, которая перечисляется пользователю
            var userSum = sum - comissionSum;
            //Результат ответа от яндекса
            try
            {
                var result = await yaService.PayOutAsync(userSum, user.Id);
                if (result.Succeeded)
                {
                    //Сохраняем операцию в базу
                    var operationId = (string)result.ResultData;
                    await paymentService.AddAsync(-sum, operationId, user.Id, PaymentType.YandexWallet);

                    if(user.RefererId != null)
                    {
                        //Перечисляем рефералу
                        await cashService.AddAsync(user.Id, user.RefererId, comissionSum / 2, CashOperationTypes.Referal);
                        //Перечисляем системе
                        await cashService.AddAsync(user.Id, null, comissionSum / 2, CashOperationTypes.Comission);
                    }
                    else
                    {
                        //Перечисляем только системе
                        await cashService.AddAsync(user.Id, null, comissionSum, CashOperationTypes.Comission);
                    }
                    return Json(JsonStatuses.Success);
                }
                else
                {
                    return Json(JsonStatuses.ValidationError, result.Errors);
                }
            }
            catch(InvalidTokenException e)
            {
                var userLogins = userManager.GetLogins(user.Id);
                var yaLogin = userLogins.FirstOrDefault(x => x.LoginProvider == YaMoneySettings.DefaultAuthType);
                logger.Error(e, "Истек срок Яндекс токена.");
                externalServices.Remove(user.Id, YaMoneySettings.DefaultAuthType);

                return Json(JsonStatuses.ValidationError, new [] { "Истек срок токена, перезагрузите страницу и попробуйте снова авторизовать приложение" });
            }
        }
        /// <summary>
        /// Первый из 3х шагов, 2 остальных в YandexController
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PayInBank(BankPayInModel model)
        {
            var sum = (double)model.BankPayInSum;
            var userId = User.Identity.GetUserId();

            var sessionPayment = new ExternalPayment
            {
                Sum = sum
            };
            var result = await yaService.ExternalPayIn(sum, userId, sessionPayment);

            if (result.ResultData != null && result.Succeeded)
            {
                if (result.ResultData is ProcessExternalPaymentResult procResult)
                {
                    var getParams = DictionaryToParams(procResult.AcsParams);

                    Session[SessionKeys.YaMoneyExternal] = sessionPayment;
                    return Json(JsonStatuses.Success, procResult.AcsUri + "?" + getParams);
                }
                logger.Error("Ошибка приведения типов: ProcessExternalPaymentResult.");
            }
            return Json(JsonStatuses.ValidationError, new[] { "Техническая ошибка" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PayInWebMoney(WmPayInModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            }
            var result = await wmService.PayInAsync(model.WmPhoneNumber, (double)model.WmPayInSum);
            if (result.Succeeded)
            {
                if (result.ResultData == null)
                    return Json(JsonStatuses.Error);
                Session[SessionKeys.WebMoneyExpressInvoiceId] = (int)result.ResultData;
                return Json(JsonStatuses.Success);
            }
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmPayInWebmoney(WmPayInConfirmModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            }
            var result = await wmService.ConfirmPayInAsync((int)Session[SessionKeys.WebMoneyExpressInvoiceId], model.WmConfirmCode, User.Identity.GetUserId());
            if (result.Succeeded)
            {
                //В ResultData лежит сообщение от webmoney
                return Json(JsonStatuses.Success, result.ResultData as string);
            }
            return Json(JsonStatuses.ValidationError, result.Errors);

        }
        private string DictionaryToParams(IDictionary<string, string> dict)
        {
            var list = new List<string>();
            foreach(var pair in dict)
            {
                list.Add(pair.Key + "=" + pair.Value);
            }
            return string.Join("&", list);
        }
    }
}