using Mite.Constants;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Mite.Attributes.Filters
{
    public class ValidateReCaptchaAttribute : FilterAttribute, IAuthorizationFilter
    {
        public HttpClient HttpClient { get; set; }

        public async void OnAuthorization(AuthorizationContext filterContext)
        {
            var captcha = filterContext.HttpContext.Request.Form["g-recaptcha-response"];

            if (string.IsNullOrEmpty(captcha))
            {
                filterContext.Controller.ViewData.ModelState.AddModelError("", "Заполните капчу.");
                return;
            }

            var reqParams = new Dictionary<string, string>();
            reqParams.Add("secret", GoogleCaptchaSettings.ReCaptchaSecret);
            reqParams.Add("response", captcha);

            var content = new FormUrlEncodedContent(reqParams);
            var result = await HttpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            var resultData = JObject.Parse(await result.Content.ReadAsStringAsync());
            var resultSuccess = (bool)resultData["success"];
            if (!resultSuccess)
            {
                filterContext.Controller.ViewData.ModelState.AddModelError("", "Ошибка ReCaptcha");
            }
        }
    }
}