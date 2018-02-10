using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    public class BlackListController : ApiController
    {
        private readonly BlackListUserRepository repository;
        private readonly AppUserManager userManager;
        private readonly ILogger logger;

        public BlackListController(IUnitOfWork unitOfWork, AppUserManager userManager, ILogger logger)
        {
            repository = unitOfWork.GetRepo<BlackListUserRepository, BlackListUser>();
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Add([FromBody]string targetId)
        {
            var target = await userManager.FindByIdAsync(targetId);
            if(target == null)
                return BadRequest("Неизвестный пользователь");

            try
            {
                await repository.AddAsync(new BlackListUser
                {
                    ListedUserId = targetId,
                    CallerId = User.Identity.GetUserId()
                });
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при удалении из черного списка: {e.Message}");
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove(string targetId)
        {
            var listItem = await repository.GetAsync(User.Identity.GetUserId(), targetId);
            if (listItem == null)
                return BadRequest("Совпадение не найдено");
            try
            {
                await repository.RemoveAsync(listItem);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при удалении из черного списка: {e.Message}");
                return InternalServerError();
            }
        }
    }
}