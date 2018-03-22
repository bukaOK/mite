using Microsoft.AspNet.Identity;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    [Authorize]
    public class UserTagsController : ApiController
    {
        private readonly UserRepository userRepository;
        private readonly ILogger logger;

        public UserTagsController(IUnitOfWork unitOfWork, ILogger logger)
        {
            userRepository = unitOfWork.GetRepo<UserRepository, User>();
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add(Guid tagId)
        {
            try
            {
                await userRepository.AddTagAsync(tagId, User.Identity.GetUserId());
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка во время добавления тега пользователя: {e.Message}");
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove(Guid tagId)
        {
            try
            {
                await userRepository.RemoveTagAsync(tagId, User.Identity.GetUserId());
                return Ok();
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка во время удаления тега пользователя: {e.Message}");
                return InternalServerError();
            }
        }
    }
}