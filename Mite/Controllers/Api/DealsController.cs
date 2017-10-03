using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    public class DealsController : ApiController
    {
        private readonly IDealService dealService;

        public DealsController(IDealService dealService)
        {
            this.dealService = dealService;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Create(DealFormModel model)
        {
            var result = await dealService.CreateAsync(model.Id, User.Identity.GetUserId());
            if (result.Succeeded)
            {
                var dealId = (long)result.ResultData;
                return Ok(dealId);
            }
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update(DealModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (model.Deadline != null && (model.Deadline.Value.DayOfYear < DateTime.Now.DayOfYear ||
                model.Deadline.Value.Year < DateTime.Now.Year))
            {
                ModelState.AddModelError("EndDateStr", "Дата не может быть меньше текущей");
                return BadRequest(ModelState);
            }
            model.Author = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            model.Status = DealStatuses.ExpectPayment;
            var result = await dealService.UpdateNewAsync(model, User.Identity.GetUserId());
            if (result.Succeeded)
                return Ok();
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove(long id)
        {
            var result = await dealService.RemoveAsync(id);
            if (result.Succeeded)
                return Ok();
            return Content(HttpStatusCode.ServiceUnavailable, result.Errors);
        }
    }
    public class DealFormModel
    {
        public Guid Id { get; set; }
    }
}
