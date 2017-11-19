using AutoMapper;
using Mite.ExternalServices.WebMoney.Core;
using Mite.ExternalServices.WebMoney.Params;
using Mite.ExternalServices.WebMoney.Requests;
using Mite.ExternalServices.WebMoney.Responses;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney
{
    public class WMExpressPayment : WMPayment
    {
        public WMExpressPayment(ILogger logger, HttpClient client) : base(client, logger)
        {
        }
        /// <summary>
        /// Производит экспресс оплату
        /// </summary>
        /// <param name="clientPhoneStr"></param>
        /// <returns>Номер счёта или null</returns>
        public async Task<WmPaymentResult> PayInAsync(ExpressPaymentParams paymentParams)
        {
            var request = Mapper.Map<ExpressPaymentRequest>(paymentParams);
            request.StorePurse = WmrPurse;
            //request.Description = "Пополнение счёта MiteGroup.";
            request.Description = "X20 test payment";
            //request.EmulatedFlag = 1;
            try
            {
                var resp = await PostAsync<ExpressPaymentResponse>(request);
                if(resp.ErrorCode != 0)
                {
                    Logger.Warn($"Webmoney: Ошибка при попытке пополнить счёт. Код: {resp.ErrorCode}. Описание: {resp.ErrorDescription}");
                    return WmPaymentResult.Failed(resp.UserDescription);
                }
                return WmPaymentResult.Success(resp.Operation.InvoiceId);
            }
            catch(Exception e)
            {
                Logger.Error($"WebMoney: Ошибка при попытке пополнить счёт: " + e.Message);
                return WmPaymentResult.Failed("Ошибка при попытке пополнить счёт.");
            }
        }
        public async Task<WmPaymentResult> ConfirmPayInAsync(ExpressPaymentConfirmParams paymentParams)
        {
            var request = Mapper.Map<ExpressPaymentConfirmRequest>(paymentParams);
            request.StorePurse = WmrPurse;
            try
            {
                var resp = await PostAsync<ExpressPaymentConfirmResponse>(request);
                if(resp.ErrorCode > 0)
                {
                    Logger.Warn("Webmoney: Ошибка при попытке пополнить счёт: " + resp.ErrorDescription);
                    return WmPaymentResult.Failed(resp.UserDescription);
                }
                return WmPaymentResult.Success(resp);
            }
            catch(Exception e)
            {
                Logger.Error($"WebMoney: Ошибка при попытке подтвердить пополнение счёта: " + e.Message);
                return WmPaymentResult.Failed("Ошибка при попытке пополнить счёт.");
            }
        }
    }
}
