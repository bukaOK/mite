using Microsoft.AspNet.Identity;
using Mite.BLL.Core;
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

namespace Mite.Controllers
{
    [Authorize]
    public class PaymentsController : BaseController
    {
        private const double Comission = 0.02;

        private readonly AppUserManager userManager;
        private readonly IYandexMoneyService yaService;
        private readonly IPaymentService paymentService;
        private readonly ICashService cashService;
        private readonly ILogger logger;
        private readonly IExternalServices externalServices;

        public PaymentsController(AppUserManager userManager, IYandexMoneyService yaService, IPaymentService paymentService,
            ICashService cashService, ILogger logger, IExternalServices externalServices)
        {
            this.userManager = userManager;
            this.yaService = yaService;
            this.paymentService = paymentService;
            this.cashService = cashService;
            this.logger = logger;
            this.externalServices = externalServices;
        }
        public PartialViewResult PayIn()
        {
            var model = new PayInModel
            {
                PaymentType = (byte)PaymentType.YandexWallet
            };
            return PartialView(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PayIn(PayInModel model)
        {
            var sum = (double)model.PayInSum;
            var userId = User.Identity.GetUserId();

            DataServiceResult result;
            switch ((PaymentType)model.PaymentType)
            {
                case PaymentType.YandexWallet:
                    try
                    {
                        result = await yaService.PayInAsync(sum);
                        if (result.Succeeded)
                        {
                            //Сохраняем операцию в базу
                            var operationId = (string)result.ResultData;
                            await paymentService.AddAsync(sum, operationId, userId, (PaymentType)model.PaymentType);
                            return Json(JsonStatuses.Success);
                        }
                        else
                        {
                            return Json(JsonStatuses.ValidationError, result.Errors);
                        }
                    }
                    catch (InvalidTokenException e)
                    {
                        var userLogins = userManager.GetLogins(userId);
                        var yaLogin = userLogins.FirstOrDefault(x => x.LoginProvider == YaMoneySettings.DefaultAuthType);
                        logger.Error(e, "Истек срок Яндекс токена.");
                        externalServices.Remove(userId, YaMoneySettings.DefaultAuthType);
                        return Json(JsonStatuses.ValidationError, new string[] { "Истек срок токена, перезагрузите страницу и попробуйте снова авторизовать приложение" });
                    }
                case PaymentType.BankCard:
                    var sessionPayment = new ExternalPayment
                    {
                        Sum = sum
                    };
                    result = await yaService.ExternalPayIn(sum, userId, sessionPayment);

                    if(result.ResultData != null && result.Succeeded)
                    {
                        if (result.ResultData is ProcessExternalPaymentResult procResult)
                        {
                            var getParams = DictionaryToParams(procResult.AcsParams);

                            Session[SessionKeys.YaMoneyExternal] = sessionPayment;
                            return Json(JsonStatuses.Success, procResult.AcsUri + "?" + getParams);
                        }
                        logger.Error("Ошибка приведения типов: ProcessExternalPaymentResult.");
                        return Json(JsonStatuses.ValidationError, new string[] { "Техническая ошибка" });
                    }
                    return Json(JsonStatuses.ValidationError, new string[] { "Техническая ошибка" });
                default:
                    return Json(JsonStatuses.Error);
            }
        }
        public PartialViewResult PayOut()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PayOut(PayOutModel model)
        {
            if(model.PayOutSum == 0 || model.PayOutSum == null)
            {
                return Json(JsonStatuses.ValidationError, new[] { "Сумма не может быть 0." });
            }
            if(model.PayOutSum < 500)
            {
                return Json(JsonStatuses.ValidationError, new[] { "Минимальная сумма для вывода - 500 руб." });
            }
            var sum = (double)model.PayOutSum;
            var userCash = await cashService.GetUserCashAsync(User.Identity.GetUserId());

            if (sum > userCash)
            {
                return Json(JsonStatuses.ValidationError, new string[] { "Требуемая сумма больше вашего баланса." });
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

                return Json(JsonStatuses.ValidationError, new string[] { "Истек срок токена, перезагрузите страницу и попробуйте снова авторизовать приложение" });
            }
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