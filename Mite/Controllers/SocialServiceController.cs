using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers
{
    [Authorize(Roles = "admin")]
    public class SocialServiceController : ApiController
    {
        private readonly SocialServicesRepository repo;
        private readonly ILogger logger;

        public SocialServiceController(IUnitOfWork unitOfWork, ILogger logger)
        {
            repo = unitOfWork.GetRepo<SocialServicesRepository, SocialService>();
            this.logger = logger;
        }
        [HttpGet]
        public Task<IEnumerable<SocialService>> GetAll()
        {
            return repo.GetAllAsync();
        }
        [HttpGet]
        public Task<SocialService> Get(string name)
        {
            return repo.GetAsync(name);
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add([FromBody] SocialService socialService)
        {
            try
            {
                await repo.AddAsync(socialService);
                return Ok();
            }
            catch(Exception e)
            {
                var msg = "Ошибка при добавлении соц. сервиса";
                logger.Error(e, msg);
                return InternalServerError();
            }
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update([FromBody] SocialService socialService)
        {
            try
            {
                await repo.UpdateAsync(socialService);
                return Ok();
            }
            catch(Exception e)
            {
                var msg = "Ошибка при обновлении соц. сервиса";
                logger.Error(e, msg);
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string name)
        {
            try
            {
                var entity = await repo.GetAsync(name);
                await repo.RemoveAsync(entity);
                return Ok();
            }
            catch (Exception e)
            {
                var msg = "Ошибка при удалении соц. сервиса";
                logger.Error(e, msg);
                return InternalServerError();
            }
        }
    }
}