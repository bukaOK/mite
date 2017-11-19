using Mite.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mite.BLL.Services;
using Microsoft.AspNet.Identity;
using Mite.Core;
using System.Web.Mvc;
using Mite.CodeData.Enums;

namespace Mite.Controllers
{
    [Authorize]
    public class RatingController : BaseController
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }
        [HttpPost]
        public async Task<ActionResult> RatePost(PostRatingModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                postModel.UserId = User.Identity.GetUserId();
                await _ratingService.RatePostAsync(postModel);
                return Ok();
            }
            catch(Exception)
            {
                return InternalServerError();
            }
        }
        [HttpPost]
        public async Task<ActionResult> RateComment(CommentRatingModel commentModel)
        {
            //Фиксированная оцена комментария
            const int maxRateVal = 1;
            const int minRateVal = 0;
            if (!ModelState.IsValid || commentModel.Value < minRateVal || commentModel.Value > maxRateVal)
            {
                return BadRequest();
            }
            try
            {
                commentModel.UserId = User.Identity.GetUserId();
                await _ratingService.RateCommentAsync(commentModel);
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
        [HttpPost]
        public async Task<ActionResult> Recount(Guid id, RatingRecountTypes recountType)
        {
            var result = await _ratingService.RecountAsync(id, recountType);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
    }
}
