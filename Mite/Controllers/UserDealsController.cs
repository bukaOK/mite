using Microsoft.AspNet.Identity;
using Mite.Attributes.Filters;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [AjaxOnly("Index")]
    [Route("user/deals/{action=index}/{type?}")]
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
        public async Task<ActionResult> Incoming(DealStatuses type)
        {
            var deals = await dealService.GetIncomingAsync(type, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, deals, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Outgoing(DealStatuses type)
        {
            var deals = await dealService.GetOutgoingAsync(type, User.Identity.GetUserId());
            return Json(JsonStatuses.Success, deals, JsonRequestBehavior.AllowGet);
        }
    }
}