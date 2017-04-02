using Microsoft.AspNet.Identity;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using System.Linq;
using AutoMapper;
using Mite.Enums;

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
                FollowingUserId = followingId
            };
            follower.Id = User.Identity.GetUserId();
            follower.FollowTime = DateTime.UtcNow;
            try
            {
                await _unitOfWork.FollowersRepository.AddAsync(follower);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
