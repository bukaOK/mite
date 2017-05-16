using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Mite.DAL.Core
{
    //TODO: проверка Guid на уникальность
    public abstract class Repository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly IDbConnection Db;
        private readonly string _tableName;

        protected Repository(IDbConnection db)
        {
            Db = db;
            _tableName = typeof(T).Name + "s";
        }
        
        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return Db.QueryAsync<T>($"select * from dbo.{_tableName}");
        }

        public virtual Task<T> GetAsync(Guid id)
        {
            return Db.QueryFirstAsync<T>($"select top 1 * from dbo.{_tableName} where Id=@Id", new { Id = id });
        }
        public virtual Task RemoveAsync(Guid id)
        {
            var query = $"delete from dbo.{_tableName} where Id=@Id";
            return Db.ExecuteAsync(query, new { Id = id });
        }
        public virtual Task AddAsync(T entity)
        {
            //Если Id пустой, генерим новый
            entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

            //Составляем список свойств для вставки
            var propsNames = typeof(T).GetProperties().Where(IsSqlType).Select(x => x.Name).ToArray();
            var query = new StringBuilder($"insert into dbo.{_tableName} (");
            for (int i = 0; i < propsNames.Length; i++)
            {
                query.Append(propsNames[i]);
                if (i < propsNames.Length - 1)
                    query.Append(",");
            }
            query.Append(") values(");
            for (int i = 0; i < propsNames.Length; i++)
            {
                query.Append("@").Append(propsNames[i]);
                if (i != propsNames.Length - 1)
                    query.Append(",");
            }
            query.Append(")");
            return Db.ExecuteAsync(query.ToString(), entity);
        }

        public virtual Task UpdateAsync(T entity)
        {
            var propsNames =
                typeof(T).GetProperties()
                    .Where(x => IsSqlType(x) && x.Name != "Id" && x.GetValue(entity) != null)
                    .Select(x => x.Name)
                    .ToArray();
            var query = new StringBuilder($"update dbo.{_tableName} set ");
            for (int i = 0; i < propsNames.Length; i++)
            {
                query.AppendFormat("{0}=@{0}", propsNames[i]);
                if (i < propsNames.Length - 1)
                    query.Append(",");
            }
            query.Append(" where Id=@Id");
            return Db.ExecuteAsync(query.ToString(), entity);
        }

        /// <summary>
        /// не все типы проверены
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static bool IsSqlType(PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            return propType == typeof(string) || propType.IsPrimitive || propType.IsValueType;
        }

        public IEnumerable<T> GetAll()
        {
            return Db.Query<T>($"select * from dbo.{_tableName}");
        }

        public void Add(T entity)
        {
            //Если Id пустой, генерим новый
            entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

            //Составляем список свойств для вставки
            var propsNames = typeof(T).GetProperties().Where(IsSqlType).Select(x => x.Name).ToArray();
            var query = new StringBuilder($"insert into dbo.{_tableName} (");
            for (int i = 0; i < propsNames.Length; i++)
            {
                query.Append(propsNames[i]);
                if (i < propsNames.Length - 1)
                    query.Append(",");
            }
            query.Append(") values(");
            for (int i = 0; i < propsNames.Length; i++)
            {
                query.Append("@").Append(propsNames[i]);
                if (i != propsNames.Length - 1)
                    query.Append(",");
            }
            query.Append(")");
            Db.Execute(query.ToString(), entity);
        }

        public T Get(Guid id)
        {
            return Db.QueryFirst<T>($"select top 1 * from dbo.{_tableName} where Id=@Id", new { Id = id });
        }

        public void Remove(Guid id)
        {
            var query = $"delete from dbo.{_tableName} where Id=@Id";
            Db.Execute(query, new { Id = id });
        }
    }
}