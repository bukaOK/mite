using System.Collections.Generic;

namespace Mite.Core
{
    /// <summary>
    /// Интерфейс "захардкоженного" хранилища
    /// данных, которые нет смысла хранить в базе
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataSource<T>
    {
        /// <summary>
        /// Возвращает список данных
        /// </summary>
        /// <returns></returns>
        List<T> GetItems();

        /// <summary>
        /// Возвращает одно из данных по ID
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        T GetItem(int itemId);
    }
    /// <summary>
    /// Абстрактный "захардкоженного" хранилища
    /// данных которые нет смысла хранить данных
    /// </summary>
    /// <typeparam name="T">Тип хранимых объектов(например SelectListItem)</typeparam>
    public abstract class DataSource<T> : IDataSource<T>
    {
        public abstract List<T> GetItems();

        public virtual T GetItem(int id)
        {
            return GetItems()[id];
        }
    }
}