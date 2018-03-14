using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    public class UserReviewsController : ApiController
    {
        private readonly IUserReviewService reviewService;

        public UserReviewsController(IUserReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Add(UserReviewModel model)
        {
            var result = await reviewService.AddAsync(User.Identity.GetUserId(), model.Review);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
    }
}