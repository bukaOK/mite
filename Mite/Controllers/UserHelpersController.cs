using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Enums;
using Mite.Infrastructure;
using NLog;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class UserHelpersController : BaseController
    {
        private readonly IHelpersService helpersService;
        private readonly ILogger logger;

        public UserHelpersController(IHelpersService helpersService, ILogger logger)
        {
            this.helpersService = helpersService;
            this.logger = logger;
        }
        /// <summary>
        /// Удаляем использованного помощника
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCodeResult> InitHelper(HelperTypes helperType)
        {
            var result = await helpersService.InitHelperAsync(helperType, User.Identity.GetUserId());
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                var errorMsg = "";
                foreach(var err in result.Errors)
                {
                    errorMsg += err + Environment.NewLine;
                }
                logger.Error(errorMsg);
                return InternalServerError();
            }
        }
        public async Task<JsonResult> GetHelper()
        {
            var result = await helpersService.GetByUserAsync(User.Identity.GetUserId());
            return Json(JsonStatuses.Success, result);
        }
    }
}