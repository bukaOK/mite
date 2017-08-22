using Mite.BLL.Core;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Constants;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Yandex.Money.Api.Sdk.Interfaces;
using Yandex.Money.Api.Sdk.Requests;
using Yandex.Money.Api.Sdk.Responses;
using Yandex.Money.Api.Sdk.Responses.Base;
using Yandex.Money.Api.Sdk.Utils;

namespace Mite.ExternalServices.YandexMoney
{
    public interface IYandexMoneyService : IDataService
    {
        Task AuthorizeAsync(string userId, string code);
        Task<string> GetTokenAsync(string userId);
        /// <summary>
        /// Отправляет запрос в яндекс.деньги на перечисление с аккаунта системы
        /// </summary>
        /// <param name="sum"></param>
        /// <param name="userId">Кому отправляем средства</param>
        /// <returns>DataServiceResult с Id запроса в яндексе</returns>
        Task<DataServiceResult> PayOutAsync(double sum, string userId);
        /// <summary>
        /// Перечисление на аккаунт системы
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        Task<DataServiceResult> PayInAsync(double sum, string userId);
        Task<DataServiceResult> ExternalPayIn(double sum, string userId, ExternalPayment sessionPayment);
        Task<DataServiceResult> ExternalPayIn(string requestId, string instanceId);
    }
    public class YandexMoneyService : DataService, IYandexMoneyService
    {
        private readonly AppUserManager userManager;
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private Authenticator authenticator;

        public YandexMoneyService(IUnitOfWork unitOfWork, AppUserManager userManager, IHttpClient httpClient, 
            ILogger logger, Authenticator authenticator) : base(unitOfWork)
        {
            this.userManager = userManager;
            this.httpClient = httpClient;
            this.logger = logger;
            this.authenticator = authenticator;
        }
        public async Task AuthorizeAsync(string userId, string code)
        {
            var tr = new TokenRequest<TokenResult>(httpClient, new JsonSerializer<TokenResult>())
            {
                Code = code,
                ClientId = YaMoneySettings.ClientId,
                RedirectUri = YaMoneySettings.RedirectUri,
                ClientSecret = YaMoneySettings.Secret
            };

            var token = await tr.Perform();

            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            //Добавляем или обновляем токен в базе
            var existingService = await repo.GetAsync(userId, YaMoneySettings.DefaultAuthType);
            if(existingService == null)
            {
                await repo.AddAsync(new ExternalService
                {
                    Name = YaMoneySettings.DefaultAuthType,
                    AccessToken = token.Token,
                    UserId = userId
                });
            }
            else
            {
                existingService.AccessToken = token.Token;
                await repo.UpdateAsync(existingService);
            }
            authenticator.Token = token.Token;

            var accountInfoReq = new AccountInfoRequest<AccountInfoResult>(httpClient, new JsonSerializer<AccountInfoResult>());
            var accountInfoRes = await accountInfoReq.Perform();
            var user = await userManager.FindByIdAsync(userId);
            user.YandexWalId = accountInfoRes.Account;
            await userManager.UpdateAsync(user);
        }
        /// <summary>
        /// Ввод средств Яндекс.Деньги
        /// </summary>
        /// <param name="sum">Сумма взноса</param>
        /// <param name="userId">От кого</param>
        /// <returns></returns>
        public async Task<DataServiceResult> PayInAsync(double sum, string userId)
        {
            var p2p = new P2PRequestPaymentParams
            {
                AmountDue = sum.ToString(),
                To = YaMoneySettings.WalletId
            };
            authenticator.Token = await GetTokenAsync(userId);
            var req = new RequestPaymentRequest<RequestPaymentResult>(httpClient, new JsonSerializer<RequestPaymentResult>())
            {
                PaymentParams = p2p.GetParams()
            };
            var reqPaymentResult = await req.Perform();
            if (reqPaymentResult.Status == ResponseStatus.Success)
            {
                var result = await ConfirmPayment(reqPaymentResult.RequestID);
                return result;
            }
            //Если запрос провальный, возвращаем подходящие сообщения об ощибке
            logger.Error($"Ошибка при вводе средств Яндекс.Денег. Код: {reqPaymentResult.Error}");
            return DataServiceResult.Failed(ParseError(reqPaymentResult.ErrorCode()));
        }
        /// <summary>
        /// Выводе средств
        /// </summary>
        /// <param name="sum"></param>
        /// <param name="userId">Кому перечисляем</param>
        /// <returns></returns>
        public async Task<DataServiceResult> PayOutAsync(double sum, string userId)
        {
            var p2p = new YaP2PRequestPaymentParams
            {
                Amount = sum.ToString("0.00", new CultureInfo("en-us")),
                Comment = $"Вывод средств. Пользователь {userId}",
                Message = "Вывод средств. MiteGroup."
            };
            var user = await userManager.FindByIdAsync(userId);
            //Номер счета пользователя
            var wallet = user.YandexWalId;
            if (wallet == null)
            {
                var accountReq = new AccountInfoRequest<AccountInfoResult>(httpClient, new JsonSerializer<AccountInfoResult>());
                var accountInfo = await accountReq.Perform();
                wallet = accountInfo.Account;
                user.YandexWalId = wallet;
                await userManager.UpdateAsync(user);
            }
            p2p.To = wallet;
            //от кого перечисляем
            var systemUser = await userManager.FindByNameAsync("landenor");
            authenticator.Token = await GetTokenAsync(systemUser.Id);

            var req = new RequestPaymentRequest<RequestPaymentResult>(httpClient, new JsonSerializer<RequestPaymentResult>())
            {
                PaymentParams = p2p.GetParams()
            };

            var reqPaymentResult = await req.Perform();
            if (reqPaymentResult.Status == ResponseStatus.Success)
            {
                var result = await ConfirmPayment(reqPaymentResult.RequestID);
                return result;
            }
            logger.Error($"Ошибка при выводе средств Яндекс.Денег. Код: {reqPaymentResult.Error}");
            return DataServiceResult.Failed(ParseError(reqPaymentResult.ErrorCode()));
        }
        /// <summary>
        /// Платеж с банковской карты. Первые 2 шага.
        /// </summary>
        /// <see cref="https://tech.yandex.ru/money/doc/dg/reference/process-external-payments-docpage/" />
        /// <returns></returns>
        public async Task<DataServiceResult> ExternalPayIn(double sum, string userId, ExternalPayment sessionPayment)
        {
            var instanceIdReq = new InstanceIdRequest<InstanceIdResult>(httpClient, new JsonSerializer<InstanceIdResult>())
            {
                ClientId = YaMoneySettings.ClientId
            };
            //Получаем Id приложения
            var instanceIdResp = await instanceIdReq.Perform();
            if(instanceIdResp.Status == ResponseStatus.Success)
            {
                var instanceId = instanceIdResp.InstanceId;
                sessionPayment.InstanceID = instanceId;

                var p2p = new P2PRequestPaymentParams()
                {
                    AmountDue = sum.ToString("0.00", new CultureInfo("en-us")),
                    Message = $"Пополнение средств аккаунта MiteGroup. Id пользователя {userId}",
                    To = YaMoneySettings.WalletId
                };
                //Получаем id операции
                var reqExtPayment = new RequestExternalPaymentRequest<RequestExternalPaymentResult>(httpClient,
                    new JsonSerializer<RequestExternalPaymentResult>())
                {
                    PaymentParams = p2p.GetParams(),
                    InstanceId = instanceId
                };
                var reqExtResult = await reqExtPayment.Perform();
                sessionPayment.RequestID = reqExtResult.RequestID;

                if(reqExtResult.Status == ResponseStatus.Success)
                {
                    var result = await ConfirmExternalPayment(reqExtResult.RequestID, instanceId);
                    return result;
                }
                //В случае ошибки, логгим её и возвращаем текст
                logger.Error($"Ошибка при запросе платежа с банковской карты на Яндекс кошелек. Код: {reqExtResult.Error}");
                return DataServiceResult.Failed(ParseError(reqExtResult.ErrorCode()));
            }
            logger.Error($"Ошибка при получении Id приложения с банковской карты на Яндекс кошелек. Код: {instanceIdResp.Error}");
            return DataServiceResult.Failed(ParseError(instanceIdResp.ErrorCode()));
        }
        /// <summary>
        /// Платеж с банковской карты. 4 шаг
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="instanceId"></param>
        /// <see cref="https://tech.yandex.ru/money/doc/dg/reference/process-external-payments-docpage/" />
        /// <returns></returns>
        public async Task<DataServiceResult> ExternalPayIn(string requestId, string instanceId)
        {
            var result = await ConfirmExternalPayment(requestId, instanceId);
            return result;
        }
        /// <summary>
        /// Подтверждение платежа Яндекс.Деньги
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private async Task<DataServiceResult> ConfirmPayment(string requestId, byte depth = 1)
        {
            var ppr = new ProcessPaymentRequest<ProcessPaymentResult>(httpClient, new JsonSerializer<ProcessPaymentResult>())
            {
                RequestId = requestId
            };
            var procPaymentResult = await ppr.Perform();

            switch (procPaymentResult.Status)
            {
                case ResponseStatus.Success:
                    return DataServiceResult.Success(procPaymentResult.PaymentId);
                case ResponseStatus.InProgress:
                    if (procPaymentResult.NextRetry == default(int))
                    {
                        logger.Error("Подтверждение платежа Яндекс.Деньги. Не задано время ожидания");
                        return DataServiceResult.Failed("Подтверждение платежа Яндекс.Деньги. Не задано время ожидания");
                    }
                    if (depth > 5)
                    {
                        logger.Error("Подтверждение платежа Яндекс.Деньги. Превышено кол-во запросов.");
                        return DataServiceResult.Failed("Подтверждение платежа Яндекс.Деньги. Превышено кол-во запросов.");
                    }
                    await Task.Delay(procPaymentResult.NextRetry);

                    depth++;
                    var result = await ConfirmPayment(requestId, depth);
                    return result;
                default:
                    logger.Error($"Подтверждение оплаты Яндекс.Денег. Код: {procPaymentResult.Error}");
                    return DataServiceResult.Failed($"Ошибка при подтверждении перевода Яндекс.Денег. {ParseError(procPaymentResult.ErrorCode())}");
            }
        }
        /// <summary>
        /// Подтверждение платежа с банковской карты на Яндекс кошелек
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="instanceId"></param>
        /// <param name="depth"></param>
        /// <returns>
        /// В случае требования расширенной авторизации возвращает DataServiceResult с объектом ProcessExternalPaymentResult,
        /// где содержатся параметры для редиректа на яндекс
        /// Если успех - пустой объект DataServiceResult
        /// </returns>
        private async Task<DataServiceResult> ConfirmExternalPayment(string requestId, string instanceId, bool requestToken = false, byte depth = 1)
        {
            var procPaymentReq = new ProcessExternalPaymentRequest<ProcessExternalPaymentResult>(httpClient,
                        new JsonSerializer<ProcessExternalPaymentResult>())
            {
                RequestId = requestId,
                InstanceId = instanceId,
                ExtAuthSuccessUri = YaMoneySettings.ExtPayRedirectSuccess,
                ExtAuthFailUri = YaMoneySettings.ExtPayRedirectFailed,
                RequestToken = requestToken
            };

            var procPaymentResp = await procPaymentReq.Perform();
            switch (procPaymentResp.Status)
            {
                case ResponseStatus.Success:
                    return DataServiceResult.Success();
                case ResponseStatus.ExtAuthRequired:
                    return DataServiceResult.Success(procPaymentResp);
                case ResponseStatus.InProgress:
                    if (procPaymentResp.NextRetry == default(int))
                    {
                        logger.Error("Подтверждение платежа с банковской карты на Яндекс.Деньги. Не задано время ожидания");
                        return DataServiceResult.Failed("Перевод платежа Яндекс.Деньги. Не задано время ожидания");
                    }
                    if (depth > 5)
                    {
                        logger.Error("Подтверждение платежа с банковской карты на Яндекс.Деньги. Превышено кол-во запросов.");
                        return DataServiceResult.Failed("Перевод платежа Яндекс.Деньги. Превышено кол-во запросов.");
                    }
                    await Task.Delay(procPaymentResp.NextRetry);

                    depth++;
                    var result = await ConfirmExternalPayment(requestId, instanceId, requestToken, depth);
                    return result;
                default:
                    logger.Error($"Подтверждение платежа с банковской карты на Яндекс.Деньги - {procPaymentResp.Error}");
                    return DataServiceResult.Failed(ParseError(procPaymentResp.ErrorCode()));
            }
        }
        public async Task<string> GetTokenAsync(string userId)
        {
            var senderService = await Database.GetRepo<ExternalServiceRepository, ExternalService>()
                .GetAsync(userId, YaMoneySettings.DefaultAuthType);
            return senderService.AccessToken;
        }
        private string ParseError(Error errorCode)
        {
            switch (errorCode)
            {
                case Error.NotEnoughFunds:
                    return "На счете плательщика недостаточно средств.";
                case Error.PayeeNotFound:
                    return "Получатель перевода не найден.";
                case Error.LimitExceeded:
                    return "Превышен один из лимитов на операции.";
                case Error.AccountBlocked:
                    return "Счет пользователя заблокирован.";
                case Error.AuthorizationReject:
                    return "В авторизации платежа отказано.";
                case Error.PaymentRefused:
                    return "Магазин отказал в приеме платежа.";
                default:
                    return "Техническая ошибка";
            }
        }
    }
}