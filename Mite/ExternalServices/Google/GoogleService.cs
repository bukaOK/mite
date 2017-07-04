using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Net.Http;
using Mite.Constants;
using Newtonsoft.Json.Linq;
using Mite.ExternalServices.Google.AdSense;
using Mite.ExternalServices.Google.Core;
using NLog;

namespace Mite.ExternalServices.Google
{
    public interface IGoogleService
    {
        Task<bool> RecaptchaValidateAsync(string captchaResponse);
        /// <summary>
        /// Авторизация на основе code(первая авторизация)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="redirectUri"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> AuthorizeAsync(string code, string redirectUri, string userId);
        /// <summary>
        /// Авторизация на основе refresh token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> AuthorizeAsync(string userId, string refreshToken);
        /// <summary>
        /// Получить доход за период
        /// </summary>
        /// <param name="from">С какой даты</param>
        /// <param name="to">До какой</param>
        /// <returns></returns>
        Task<double> GetAdsenseSumAsync(DateTime from, DateTime to, string userId);
        Task<string> GetRefreshTokenAsync(string userId);
    }
    public class GoogleService : DataService, IGoogleService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;

        public GoogleService(IUnitOfWork database, HttpClient httpClient, ILogger logger) : base(database)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<string> AuthorizeAsync(string code, string redirectUri, string userId)
        {
            var reqParams = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", GoogleApiSettings.ClientId },
                { "client_secret", GoogleApiSettings.ClientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };
            var content = new FormUrlEncodedContent(reqParams);
            var resp = await httpClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", content);
            if (resp.IsSuccessStatusCode)
            {
                var result = await resp.Content.ReadAsStringAsync();
                var jResult = JObject.Parse(result);
                await Database.ExternalServiceRepository.AddAsync(new DAL.Entities.ExternalService
                {
                    Name = GoogleApiSettings.DefaultAuthType,
                    AccessToken = (string)jResult["refresh_token"],
                    UserId = userId
                });
                return (string)jResult["access_token"];
            }
            logger.Error("Ошибка при авторизации Google AdSense: " + await resp.Content.ReadAsStringAsync());
            return null;
        }
        public async Task<string> AuthorizeAsync(string userId, string refreshToken)
        {
            var reqParams = new Dictionary<string, string>
            {
                { "client_id", GoogleApiSettings.ClientId },
                { "client_secret", GoogleApiSettings.ClientSecret },
                { "refresh_token", refreshToken },
                { "grant_type", "refresh_token" }
            };
            var content = new FormUrlEncodedContent(reqParams);
            var resp = await httpClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", content);
            var jResult = JObject.Parse(await resp.Content.ReadAsStringAsync());
            return (string)jResult["access_token"];
        }
        public async Task<string> GetRefreshTokenAsync(string userId)
        {
            var extService = await Database.ExternalServiceRepository.GetAsync(userId, GoogleApiSettings.DefaultAuthType);
            return extService.AccessToken;
        }
        public async Task<bool> RecaptchaValidateAsync(string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
                return false;
            var reqParams = new Dictionary<string, string>
            {
                { "secret", GoogleCaptchaSettings.ReCaptchaSecret },
                { "response", captchaResponse }
            };
            var content = new FormUrlEncodedContent(reqParams);
            var result = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            var resultData = JObject.Parse(await result.Content.ReadAsStringAsync());
            return (bool)resultData["success"];
        }
        public async Task<double> GetAdsenseSumAsync(DateTime from, DateTime to, string userId)
        {
            const string accountId = "pub-7675079294669395";

            var refreshToken = await GetRefreshTokenAsync(userId);
            var accessToken = await AuthorizeAsync(userId, refreshToken);

            var request = new ApiRequest<AdSenseReportsResult>(accessToken, httpClient)
            {
                RequestParams = new AdSenseReportsRequestParams(from, to)
                {
                    Currency = "RUB",
                    Metric = "Earnings"
                },
                RequestUrl = $"https://www.googleapis.com/adsense/v1.4/accounts/{accountId}/reports"
            };
            var result = await request.GetAsync();
            var sum = 0.0;
            if(result.Rows != null)
            {
                foreach (var row in result.Rows)
                {
                    sum += Convert.ToDouble(row);
                }
            }
            return sum;
        }
    }
}