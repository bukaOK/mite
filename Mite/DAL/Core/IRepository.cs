using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Mite.DAL.Core
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetAsync(params object[] keyValues);
        TEntity Get(params object[] keyValues);
        /// <summary>
        /// Возвращает список всех сущностей данного типа
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAllAsync();
        IEnumerable<TEntity> GetAll();
        /// <summary>
        /// Добавляет сущность
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(TEntity entity);
        void Add(TEntity entity);
        Task RemoveAsync(Guid id);
        Task RemoveAsync(TEntity entity);
        void Remove(Guid id);
        void Remove(TEntity entity);
        Task UpdateAsync(TEntity entity);
        int GetCount();
        Task<int> GetCountAsync();
    }
}