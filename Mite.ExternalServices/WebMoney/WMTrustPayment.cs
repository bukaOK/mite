using Mite.ExternalServices.WebMoney.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Net.Http;
using Mite.ExternalServices.WebMoney.Params;
using AutoMapper;
using Mite.ExternalServices.WebMoney.Requests;
using Mite.ExternalServices.WebMoney.Responses;

namespace Mite.ExternalServices.WebMoney
{
    /// <summary>
    /// Реализует запросы к двум интерфейсам - X21 и X2 (на основе доверия)
    /// </summary>
    public class WMTrustPayment : WMPayment
    {
        public WMTrustPayment(HttpClient client, ILogger logger) : base(client, logger)
        {
        }
        /// <summary>
        /// Установка доверия(Х21)
        /// </summary>
        /// <returns></returns>
        public async Task<WmPaymentResult> SettingTrust(SettingTrustParams paymentParams)
        {
            var request = Mapper.Map<SettingTrustRequest>(paymentParams);
            request.StorePurse = WmrPurse;
            try
            {
                var resp = await PostAsync<SettingTrustResponse>(request);
                if(resp.ErrorCode != 0)
                {
                    Logger.Warn($"Webmoney: Ошибка при попытке установить доверие. Код: {resp.ErrorCode}. Описание: {resp.ErrorDescription}");
                    return WmPaymentResult.Failed(resp.UserDescription);
                }
                return WmPaymentResult.Success(resp.TrustInfo.PurseId);
            }
            catch(Exception e)
            {
                Logger.Error("WebMoney: Ошибка при установке доверия: " + e.Message);
                return WmPaymentResult.Failed("Ошибка при установке доверия.");
            }
        }
        /// <summary>
        /// Подтверждение доверия(X21)
        /// </summary>
        /// <returns></returns>
        public async Task<WmPaymentResult> ConfirmTrust(ConfirmTrustParams trustParams)
        {
            var request = Mapper.Map<ConfirmTrustRequest>(trustParams);
            try
            {
                var resp = await PostAsync<ConfirmTrustResponse>(request);
                if (resp.ErrorCode != 0)
                {
                    Logger.Warn($"Webmoney: Ошибка при подтверждении доверия. Код: {resp.ErrorCode}. Описание: {resp.ErrorDescription}");
                    return WmPaymentResult.Failed(resp.UserDescription);
                }
                return WmPaymentResult.Success();
            }
            catch(Exception e)
            {
                Logger.Error("WebMoney: Ошибка при подтверждении доверия: " + e.Message);
                return WmPaymentResult.Failed("Ошибка при подтверждении доверия.");
            }
        }
    }
}
