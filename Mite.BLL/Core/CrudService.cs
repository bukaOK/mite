using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.DAL.Core;
using AutoMapper;

namespace Mite.BLL.Core
{
    public interface ICrudService<TModel, TRepo, TEntity>
        where TModel: class, new()
        where TEntity: class, new()
        where TRepo : IRepository<TEntity>
    {
        Task<DataServiceResult> AddAsync(TModel model);
        Task<DataServiceResult> RemoveAsync(params object[] keys);
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetAsync(params object[] keys);
        Task<DataServiceResult> UpdateAsync(TModel model);
    }

    public abstract class CrudService<TModel, TRepo, TEntity> : DataService, ICrudService<TModel, TRepo, TEntity>
        where TModel: class, new()
        where TEntity: class, new()
        where TRepo : IRepository<TEntity>
    {
        protected readonly TRepo Repo;

        public CrudService(IUnitOfWork database) : base(database)
        {
            Repo = Database.GetRepo<TRepo, TEntity>();
        }

        public async Task<DataServiceResult> AddAsync(TModel model)
        {
            try
            {
                await Repo.AddAsync(Mapper.Map<TEntity>(model));
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                //logger
                return DataServiceResult.Failed();
            }
        }

        public async Task<IEnumerable<TModel>> GetAllAsync()
        {
            var entities = await Repo.GetAllAsync();
            return Mapper.Map<IEnumerable<TModel>>(entities);
        }

        public async Task<TModel> GetAsync(params object[] keys)
        {
            var entity = await Repo.GetAsync(keys);
            return Mapper.Map<TModel>(entity);
        }

        public async Task<DataServiceResult> RemoveAsync(params object[] keys)
        {
            var entity = await Repo.GetAsync(keys);
            try
            {
                await Repo.RemoveAsync(entity);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                //log
                return DataServiceResult.Failed("Ошибка при удалении");
            }
        }

        public abstract Task<DataServiceResult> UpdateAsync(TModel model);
    }
}
