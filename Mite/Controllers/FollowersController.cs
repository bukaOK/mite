using Microsoft.AspNet.Identity;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using Mite.Infrastructure;

namespace Mite.Controllers
{
    public class FollowersController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FollowersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add([FromBody]string followingId)
        {
            if (string.IsNullOrEmpty(followingId))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            var follower = new Follower
            {
                Id = Guid.NewGuid(),
                FollowingUserId = followingId,
                UserId = User.Identity.GetUserId()
        };
            follower.Id = Guid.NewGuid();
            follower.UserId = User.Identity.GetUserId();
            follower.FollowTime = DateTime.UtcNow;
            try
            {
                await _unitOfWork.FollowersRepository.AddAsync(follower);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
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
                await _unitOfWork.FollowersRepository.RemoveAsync(followerId, followingId);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
