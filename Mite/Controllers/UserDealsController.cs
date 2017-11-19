using Microsoft.AspNet.Identity;
using Mite.Attributes.Filters;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [AjaxOnly("Index")]
    [Route("user/deals/{action=index}/{type?}")]
    [Authorize]
    public class UserDealsController : BaseController
    {
        private readonly IDealService dealService;

        public UserDealsController(IDealService dealService)
        {
            this.dealService = dealService;
        }
        public ViewResult Index()
        {
            return View("UserDeals");
        }
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ActionResult> Incoming(DealStatuses type)
        {
            var deals = await dealService.GetIncomingAsync(type, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, deals);
        }
        public async Task<ActionResult> Outgoing(DealStatuses type)
        {
            var deals = await dealService.GetOutgoingAsync(type, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, deals);
        }
        [Authorize(Roles = RoleNames.Moderator)]
        public async Task<ActionResult> Moder(DealStatuses type)
        {
            var deals = await dealService.GetForModerAsync(type, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, deals);
        }
    }
}