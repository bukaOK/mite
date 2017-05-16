using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Mite.DAL.Core
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Возвращает сущность по ID
        /// </summary>
        /// <param name="id">Id сущности</param>
        /// <returns></returns>
        Task<T> GetAsync(Guid id);
        T Get(Guid id);
        /// <summary>
        /// Возвращает список всех сущностей данного типа
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetAll();
        /// <summary>
        /// Добавляет сущность
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task AddAsync(T entity);
        void Add(T entity);
        Task RemoveAsync(Guid id);
        void Remove(Guid id);
        Task UpdateAsync(T entity);
    }
}