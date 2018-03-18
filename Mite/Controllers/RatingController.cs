using Mite.Models;
using System;
using System.Threading.Tasks;
using Mite.BLL.Services;
using Microsoft.AspNet.Identity;
using Mite.Core;
using System.Web.Mvc;
using Mite.CodeData.Enums;
using Mite.Hubs.Clients;
using System.Linq;

namespace Mite.Controllers
{
    [Authorize]
    public class RatingController : BaseController
    {
        private readonly IRatingService ratingService;
        private readonly IBlackListService blackListService;
        private readonly IPostsService postsService;

        public RatingController(IRatingService ratingService, IBlackListService blackListService, IPostsService postsService)
        {
            this.ratingService = ratingService;
            this.blackListService = blackListService;
            this.postsService = postsService;
        }
        [HttpPost]
        public async Task<ActionResult> RatePost(PostRatingModel postModel)
        {
            postModel.UserId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, "Ошибка валидации");

            if (!(await blackListService.CanRatePostAsync(postModel)))
                return Json(JsonStatuses.ValidationError, "Пользователь в черном списке");
            try
            {
                var result = await ratingService.RatePostAsync(postModel);
                if (result.Succeeded)
                {
                    if (postModel.IsTop)
                        await postsService.AddViewsAsync(postModel.PostId);
                    return Json(JsonStatuses.Success);
                }
                return Json(JsonStatuses.ValidationError, result.Errors.FirstOrDefault());
            }
            catch(Exception)
            {
                return InternalServerError();
            }
        }
        [HttpPost]
        public async Task<ActionResult> RateComment(CommentRatingModel commentModel)
        {
            commentModel.UserId = User.Identity.GetUserId();
            //Фиксированная оцена комментария
            const int maxRateVal = 1;
            const int minRateVal = 0;
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, "Ошибка валидации");

            if (commentModel.Value < minRateVal || commentModel.Value > maxRateVal)
                return Json(JsonStatuses.ValidationError, "Неверная оценка");

            if (!(await blackListService.CanRateCommentAsync(commentModel)))
                return Json(JsonStatuses.ValidationError, "Пользователь в черном списке");
            try
            {
                await ratingService.RateCommentAsync(commentModel);
                return Json(JsonStatuses.Success);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
        [HttpPost]
        public async Task<ActionResult> Recount(Guid id, RatingRecountTypes recountType)
        {
            var result = await ratingService.RecountAsync(id, recountType);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
    }
}
