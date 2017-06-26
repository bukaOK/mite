using Microsoft.AspNet.Identity;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using NLog;

namespace Mite.Controllers
{
    public class FollowersController : ApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger logger;

        public FollowersController(IUnitOfWork unitOfWork, ILogger logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add([FromBody]string followingId)
        {
            if (string.IsNullOrEmpty(followingId))
            {
                return BadRequest();
            }
            var follower = new Follower
            {
                FollowingUserId = followingId,
                UserId = User.Identity.GetUserId()
            };
            follower.UserId = User.Identity.GetUserId();
            follower.FollowTime = DateTime.UtcNow;
            try
            {
                var alreadyFollowed = await unitOfWork.FollowersRepository.IsFollower(follower.UserId, follower.FollowingUserId);
                if (alreadyFollowed)
                    return Ok();
                await unitOfWork.FollowersRepository.AddAsync(follower);
                return Ok();
            }
            catch (Exception e)
            {
                logger.Error(e, "Ошибка при добавлении подписчика");
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Delete([FromBody]string followingId)
        {
            if (string.IsNullOrEmpty(followingId))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            var followerId = User.Identity.GetUserId();
            try
            {
                await unitOfWork.FollowersRepository.RemoveAsync(followerId, followingId);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logger.Error(e, "Ошибка при удалении подписчика");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
