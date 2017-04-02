using Mite.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Mite.BLL.Services;
using Microsoft.AspNet.Identity;

namespace Mite.Controllers
{
    [Authorize]
    public class RatingController : ApiController
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }
        [HttpPost]
        public async Task<IHttpActionResult> RatePost(PostRatingModel postModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
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
        [HttpPut]
        public async Task<IHttpActionResult> RateComment(CommentRatingModel commentModel)
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
    }
}
