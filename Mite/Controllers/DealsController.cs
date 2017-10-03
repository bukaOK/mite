using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class DealsController : BaseController
    {
        private readonly IDealService dealService;

        public DealsController(IDealService dealService)
        {
            this.dealService = dealService;
        }
        public async Task<ActionResult> Show(long id)
        {
            var deal = await dealService.GetShowAsync(id);
            return View(deal);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pay(DealModel model)
        {
            var result = await dealService.PayAsync(model.Id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Ok();
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]        
        public async Task<ActionResult> LoadImage(DealModel model)
        {
            var result = await dealService.SaveResultImgAsync(model.Id, model.ImageSrc, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> ToExpectClient(long id)
        {
            var model = new DealModel
            {
                Status = DealStatuses.ExpectClient,
                Author = new UserShortModel
                {
                    Id = User.Identity.GetUserId()
                }
            };
            return null;
        }
        public ActionResult NewDealsCount()
        {
            var dealsCount = dealService.GetNewCount(User.Identity.GetUserId());

            if(dealsCount > 0)
                return PartialView(dealsCount);
            return new EmptyResult();
        }
    }
}