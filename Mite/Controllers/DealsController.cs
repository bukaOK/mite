using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Models;
using System.Linq;
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
            var currentUserId = User.Identity.GetUserId();
            var deal = await dealService.GetShowAsync(id);
            if (deal == null)
                return NotFound();
            if (!string.Equals(currentUserId, deal.Author.Id) && !string.Equals(currentUserId, deal.Client.Id) 
                && !User.IsInRole(RoleNames.Moderator))
                return Forbidden();
            var isAuthor = User.Identity.GetUserId() == deal.Author.Id;

            if (deal.Status == DealStatuses.Dispute && deal.Moder != null && deal.DisputeChat != null)
            {
                deal.DisputeChat.Name = "Чат спора";
                deal.DisputeChat.CurrentUser = deal.DisputeChat.Members.FirstOrDefault(x => x.Id == currentUserId);
            }
            else
            {
                deal.Chat.Name = "Чат с " + deal.Chat.Members.First(x => !string.Equals(currentUserId, x.Id)).UserName;
                //null возможен, когда модератор зашел на страницу сделки, но ещё не начал его разрешать
                deal.Chat.CurrentUser = deal.Chat.Members.FirstOrDefault(x => string.Equals(currentUserId, x.Id));
            }

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
            if (string.IsNullOrEmpty(model.ImageSrc))
                return Json(JsonStatuses.ValidationError, new[] { "Изображение не выбрано" });
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
            var result = await dealService.ConfirmAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> Reject(long id)
        {
            var result = await dealService.RejectAsync(id, User.Identity.GetUserId());
            return result.Succeeded ? Json(JsonStatuses.Success) : Json(JsonStatuses.ValidationError, result.Errors);
        }
        [Authorize(Roles = RoleNames.Moderator)]
        public async Task<ActionResult> ModerConfirm(long id, bool confirm)
        {
            var result = confirm ? await dealService.ConfirmAsync(id) : await dealService.RejectAsync(id);
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