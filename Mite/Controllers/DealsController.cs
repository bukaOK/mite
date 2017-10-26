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
            var isAuthor = User.Identity.GetUserId() == deal.Author.Id;

            deal.Chat.Name = "Диалог с " + (isAuthor ? deal.Client.UserName : deal.Author.UserName);
            deal.Chat.CurrentUser = isAuthor ? deal.Author : deal.Client;
            deal.Chat.Companion = isAuthor ? deal.Client : deal.Author;

            return View(deal);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pay(DealModel model)
        {
            var result = await dealService.PayAsync(model.Id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success, new
                {
                    payed = result.ResultData,
                    next = DealStatuses.ExpectClient
                });
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
        public async Task<ActionResult> CheckVkRepost(long id)
        {
            var result = await dealService.CheckVkRepostAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success, new
                {
                    payed = result.ResultData,
                    next = DealStatuses.ExpectClient
                });
                
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> ConfirmVkRepost(long id)
        {
            var result = await dealService.ConfirmVkRepostAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success, new
                {
                    payed = result.ResultData,
                    next = DealStatuses.ExpectClient
                });
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> Rate(long id, byte value)
        {
            var result = await dealService.RateAsync(id, value, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> GiveFeedback(DealClientModel model)
        {
            var result = await dealService.GiveFeedbackAsync(model.Id, model.Feedback, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> Confirm(long id)
        {
            var result = await dealService.ClientConfirmAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [Authorize(Roles = RoleNames.Moderator)]
        public async Task<ActionResult> ModerConfirm(long id, bool confirm)
        {
            var result = await dealService.ModerConfirmAsync(id, confirm);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> OpenDispute(long id)
        {
            var result = await dealService.OpenDisputeAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
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