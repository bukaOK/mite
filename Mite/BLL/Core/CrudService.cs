using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.DAL.Core;
using AutoMapper;
using NLog;

namespace Mite.BLL.Core
{
    public interface ICrudService<TModel, TRepo, TEntity>
        where TModel: class, new()
        where TEntity: class, new()
        where TRepo : IRepository<TEntity>
    {
        /// <summary>
        /// Мапит модель в сущность и отправляет в репозиторий на добавление
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// Объект DataServiceResult(если успешно - с моделькой внутри, иначе - с сообщением ошибки)
        /// </returns>
        Task<DataServiceResult> AddAsync(TModel model);
        /// <summary>
        /// Удаляет
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>DataServiceResult</returns>
        Task<DataServiceResult> RemoveAsync(params object[] keys);
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetAsync(params object[] keys);
        /// <summary>
        /// Обновляет
        /// </summary>
        /// <param name="model"></param>
        /// <returns>DataServiceResult (если успешно-моделька, иначе - сообщение ошибки)</returns>
        Task<DataServiceResult> UpdateAsync(TModel model);
    }

    public abstract class CrudService<TModel, TRepo, TEntity> : DataService, ICrudService<TModel, TRepo, TEntity>
        where TModel: class, new()
        where TEntity: class, new()
        where TRepo : IRepository<TEntity>
    {
        protected readonly TRepo Repo;

        public CrudService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            Repo = Database.GetRepo<TRepo, TEntity>();
        }

        public virtual async Task<DataServiceResult> AddAsync(TModel model)
        {
            try
            {
                var entity = Mapper.Map<TEntity>(model);
                await Repo.AddAsync(entity);
                return DataServiceResult.Success(Mapper.Map<TModel>(entity));
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка добавления {typeof(TEntity).Name}: {e.Message}");
                return DataServiceResult.Failed();
            }
        }

        public virtual async Task<IEnumerable<TModel>> GetAllAsync()
        {
            var entities = await Repo.GetAllAsync();
            return Mapper.Map<IEnumerable<TModel>>(entities);
        }

        public virtual async Task<TModel> GetAsync(params object[] keys)
        {
            var entity = await Repo.GetAsync(keys);
            return Mapper.Map<TModel>(entity);
        }

        public virtual async Task<DataServiceResult> RemoveAsync(params object[] keys)
        {
            var entity = await Repo.GetAsync(keys);
            try
            {
                await Repo.RemoveAsync(entity);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка удаления {typeof(TEntity).Name}: {e.Message}");
                return DataServiceResult.Failed("Ошибка при удалении");
            }
        }

        public virtual async Task<DataServiceResult> UpdateAsync(TModel model)
        {
            var entity = Mapper.Map<TEntity>(model);
            try
            {
                await Repo.UpdateAsync(entity);
                return DataServiceResult.Success(Mapper.Map<TModel>(entity));
            }
            catch(Exception e)
            {
                logger.Error($"Ошибка обновления {typeof(TEntity).Name}: {e.Message}");
                return DataServiceResult.Failed("Ошибка при обновлении");
            }
        }
    }
}
