using AutoMapper;
using Mite.CodeData.Constants;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    [Authorize(Roles = RoleNames.Moderator)]
    public class FactsController : ApiController
    {
        private readonly DailyFactsRepository factsRepository;
        private readonly ILogger logger;

        public FactsController(IUnitOfWork unitOfWork, ILogger logger)
        {
            factsRepository = unitOfWork.GetRepo<DailyFactsRepository, DailyFact>();
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IEnumerable<DailyFactModel>> GetAll()
        {
            var facts = await factsRepository.GetAllAsync();
            return Mapper.Map<IEnumerable<DailyFactModel>>(facts);
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add(DailyFactModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var fact = Mapper.Map<DailyFact>(model);
                await factsRepository.AddAsync(fact);
                return Ok(Mapper.Map<DailyFactModel>(fact));
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка при добавлении факта: {e.Message}");
                return InternalServerError();
            }
        }
        [HttpPut]
        public async Task<IHttpActionResult> Update(DailyFactModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var exFact = await factsRepository.GetAsync(model.Id);
                Mapper.Map(model, exFact);
                await factsRepository.UpdateAsync(exFact);
                return Ok(Mapper.Map<DailyFactModel>(exFact));
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка при добавлении факта: {e.Message}");
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            try
            {
                await factsRepository.RemoveAsync(id);
                return Ok();
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка при добавлении факта: {e.Message}");
                return InternalServerError();
            }
        }
    }
}