using AutoMapper;
using Mite.DAL.Core;
using Mite.DAL.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Core
{
    public abstract class CrudApiController<TEntity, TRepo, TModel> : ApiController
        where TRepo : IRepository<TEntity>
        where TEntity : class, new()
    {
        protected readonly TRepo Repo;
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly ILogger Logger;

        public CrudApiController(IUnitOfWork unitOfWork, ILogger loggger)
        {
            UnitOfWork = unitOfWork;
            Logger = loggger;
            Repo = unitOfWork.GetRepo<TRepo, TEntity>();
        }
        [HttpGet]
        public virtual async Task<IEnumerable<TModel>> Get()
        {
            var entities = await Repo.GetAllAsync();
            return Mapper.Map<IEnumerable<TModel>>(entities);
        }
        [HttpPost]
        public virtual async Task<IHttpActionResult> Add([FromBody]TModel model)
        {
            TEntity entity;
            entity = Mapper.Map<TEntity>(model);

            try
            {
                await Repo.AddAsync(entity);
            }
            catch(Exception e)
            {
                Logger.Error($"Ошибка добавления сущности {typeof(TEntity).Name}: {e.Message}");
                return InternalServerError();
            }
            return Json(Mapper.Map<TModel>(entity));
        }
        [HttpPut]
        public virtual async Task<IHttpActionResult> Update([FromBody]TModel model)
        {
            TEntity entity;
            entity = Mapper.Map<TEntity>(model);

            try
            {
                await Repo.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                Logger.Error($"Ошибка добавления сущности {typeof(TEntity).Name}: {e.Message}");
                return InternalServerError();
            }
            return Json(model);
        }
        [HttpDelete]
        public virtual async Task<IHttpActionResult> Remove([FromUri] Guid id)
        {
            try
            {
                await Repo.RemoveAsync(id);
            }
            catch (Exception e)
            {
                Logger.Error($"Ошибка добавления сущности {typeof(TEntity).Name}: {e.Message}");
                return InternalServerError();
            }
            return Ok();
        }
    }
}