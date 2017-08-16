using Mite.Core;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mite.ExternalServices.Google;
using Microsoft.AspNet.Identity;
using Mite.ExternalServices.Google.Authorize;
using NLog;

namespace Mite.Controllers
{
    public class GoogleController : BaseController
    {
        private readonly IGoogleAdSenseService googleService;
        private readonly ILogger logger;

        public GoogleController(IGoogleAdSenseService googleService, ILogger logger)
        {
            this.googleService = googleService;
            this.logger = logger;
        }
        /// <summary>
        /// Авторизация AdSense
        /// </summary>
        /// <param name="authCode"></param>
        /// <returns></returns>
        public async Task<ActionResult> Authorize(AuthCodeResponse authCode)
        {
            if (authCode.Error != null)
                logger.Error($"Ошибка при авторизации в Google Api: {authCode.Error}");
            else
            {
                var result = await googleService.AuthorizeAsync(authCode.Code, $"http://{Request.Url.Host}/google/authorize", User.Identity.GetUserId());
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}