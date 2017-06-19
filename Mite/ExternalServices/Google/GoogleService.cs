using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Mite.DAL.Infrastructure;
using System.Net.Http;
using Mite.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mite.ExternalServices.Google
{
    public interface IGoogleService
    {
        Task<bool> RecaptchaValidateAsync(string captchaResponse);
    }
    public class GoogleService : DataService, IGoogleService
    {
        private readonly HttpClient httpClient;

        public GoogleService(IUnitOfWork database, HttpClient httpClient) : base(database)
        {
            this.httpClient = httpClient;
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
    }
}