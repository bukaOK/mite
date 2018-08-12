using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderService orderService;
        private readonly IAuthorServiceTypeService typeService;
        private readonly ICityService cityService;
        private readonly ICountryService countryService;

        public OrdersController(IOrderService orderService, IAuthorServiceTypeService typeService, ICityService cityService,
            ICountryService countryService)
        {
            this.orderService = orderService;
            this.typeService = typeService;
            this.cityService = cityService;
            this.countryService = countryService;
        }
        public async Task<ActionResult> Add()
        {
            var model = new OrderEditModel
            {
                OrderTypes = await typeService.GetSelectListAsync(Guid.Empty)
            };
            return View("Edit", model);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Top()
        {
            var model = new OrderTopFilterModel
            {
                OrderTypes = await typeService.GetSelectListAsync(Guid.Empty),
                Countries = await countryService.GetSelectListAsync(CurrentUserId),
                Cities = await cityService.GetSelectListAsync(CurrentUserId)
            };
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Top(OrderTopFilterModel model)
        {
            var result = await orderService.GetTopAsync(model);
            return Json(JsonStatuses.Success, result);
        }
        [HttpPost]
        public async Task<ActionResult> Add(OrderEditModel model)
        {
            if (model.Description.Contains("<script"))
                ModelState.AddModelError("Description", "Описание содержит скрипты");
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());

            model.UserId = CurrentUserId;
            var result = await orderService.CreateAsync(model);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> Edit(string id)
        {
            if(!string.IsNullOrEmpty(id) && Guid.TryParse(id, out Guid gId))
            {
                var order = await orderService.GetToEditAsync(gId);
                if (order.UserId != CurrentUserId)
                    return Forbidden();

                order.OrderTypes = await typeService.GetSelectListAsync(Guid.Empty);
                return View(order);
            }
            return HttpNotFound();
        }
        public async Task<ActionResult> Show(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid gId))
                return HttpNotFound();
            var model = await orderService.GetAsync(gId, CurrentUserId);
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Update(OrderEditModel model)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            model.UserId = CurrentUserId;
            var result = await orderService.UpdateAsync(model);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Remove(Guid id)
        {
            var result = await orderService.RemoveAsync(id);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> ChoseExecuter(Guid orderId, string executerId)
        {
            var result = await orderService.ChoseExecuterAsync(executerId, orderId, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ActionResult> AddRequest(Guid id)
        {
            var result = await orderService.AddRequestAsync(CurrentUserId, id);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ActionResult> RemoveRequest(Guid id)
        {
            var result = await orderService.RemoveRequestAsync(CurrentUserId, id);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
    }
}