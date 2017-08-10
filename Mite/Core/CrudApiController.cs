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
        protected bool ModelIsEntity;

        public CrudApiController(IUnitOfWork unitOfWork, ILogger loggger)
        {
            UnitOfWork = unitOfWork;
            Repo = unitOfWork.GetRepo<TRepo, TEntity>();
            ModelIsEntity = typeof(TEntity) == typeof(TModel);
        }
        public virtual async Task<IEnumerable<TModel>> Get()
        {
            var entities = await Repo.GetAllAsync();
            if (ModelIsEntity)
            {
                return entities as IEnumerable<TModel>;
            }
            else
            {
                return Mapper.Map<IEnumerable<TModel>>(entities);
            }
        }
        public virtual async Task<IHttpActionResult> Post([FromBody]TModel model)
        {
            TEntity entity;
            if (!ModelIsEntity)
                entity = Mapper.Map<TEntity>(model);
            else
                entity = model as TEntity;

            try
            {
                await Repo.AddAsync(entity);
            }
            catch(Exception e)
            {
                Logger.Error($"Ошибка добавления сущности {typeof(TEntity).Name}: {e.Message}");
                return InternalServerError();
            }
            return Ok();
        }

        public virtual async Task<IHttpActionResult> Put([FromBody]TModel model)
        {
            TEntity entity;
            if (!ModelIsEntity)
                entity = Mapper.Map<TEntity>(model);
            else
                entity = model as TEntity;

            try
            {
                await Repo.UpdateAsync(entity);
            }
            catch (Exception e)
            {
                Logger.Error($"Ошибка добавления сущности {typeof(TEntity).Name}: {e.Message}");
                return InternalServerError();
            }
            return Ok();
        }
        public virtual async Task<IHttpActionResult> Delete(Guid id)
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