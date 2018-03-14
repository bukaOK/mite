using Mite.Attributes.Filters;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    [AjaxOnly("Index")]
    [Route("user/orders/{action=Index}")]
    public class UserOrdersController : BaseController
    {
        private readonly IOrderService orderService;

        public UserOrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Open()
        {
            var orders = await orderService.GetByUserAsync(CurrentUserId, OrderStatuses.Open);
            return Json(JsonStatuses.Success, orders);
        }
        public async Task<ActionResult> Chosed()
        {
            var orders = await orderService.GetByUserAsync(CurrentUserId, OrderStatuses.ExecuterChosed);
            return Json(JsonStatuses.Success, orders);
        }
        public async Task<ActionResult> Closed()
        {
            var orders = await orderService.GetByUserAsync(CurrentUserId, OrderStatuses.Closed);
            return Json(JsonStatuses.Success, orders);
        }
    }
}