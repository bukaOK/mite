using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Enums;
using Mite.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class UserHelpersController : BaseController
    {
        private readonly IHelpersService helpersService;

        public UserHelpersController(IHelpersService helpersService)
        {
            this.helpersService = helpersService;
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
                Logger.Write(EventTypes.Warning, errorMsg);
                return InternalServerError();
            }
        }
    }
}