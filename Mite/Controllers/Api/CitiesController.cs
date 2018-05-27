using AutoMapper;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Models;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using Mite.DAL.Repositories;

namespace Mite.Controllers.Api
{
    [Authorize(Roles = "moder,admin")]
    public class CitiesController : ApiController
    {
        private readonly ILogger logger;
        private readonly CitiesRepository repo;

        public CitiesController(IUnitOfWork unitOfWork, ILogger logger)
        {
            repo = unitOfWork.GetRepo<CitiesRepository, City>();
            this.logger = logger;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<CityModel>> GetByCountry(Guid id)
        {
            var cities = await repo.GetByCountryAsync(id);
            return Mapper.Map<IEnumerable<CityModel>>(cities);
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add([FromBody]CityModel model)
        {
            var entity = Mapper.Map<City>(model);

            try
            {
                await repo.AddAsync(entity);
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка добавления города: {e.Message}");
                return InternalServerError();
            }
            return Json(Mapper.Map<CityModel>(entity));
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update([FromBody]CityModel model)
        {
            var entity = Mapper.Map<City>(model);
            try
            {
                await repo.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка обновления города: {e.Message}");
                return InternalServerError();
            }
            return Json(model);
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Remove([FromUri] Guid id)
        {
            try
            {
                await repo.RemoveAsync(id);
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка удаления города: {e.Message}");
                return InternalServerError();
            }
            return Ok();
        }
    }
}