using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using System.Linq;

namespace Mite.DAL.Core
{
    //TODO: проверка Guid на уникальность
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IDbConnection Db;
        protected readonly AppDbContext DbContext;
        protected readonly IDbSet<TEntity> Table;
        protected string TableName;

        protected Repository(AppDbContext dbContext)
        {
            dbContext.Configuration.LazyLoadingEnabled = false;

            Db = dbContext.Database.Connection;
            DbContext = dbContext;
            Table = dbContext.Set<TEntity>();

            if(TableName == null)
                TableName = typeof(TEntity).Name + "s";
        }
        
        public virtual Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return Db.QueryAsync<TEntity>($"select * from dbo.\"{TableName}\";");
        }

        public virtual Task RemoveAsync(Guid id)
        {
            var query = $"delete from dbo.\"{TableName}\" where \"Id\"=@Id;";
            return Db.ExecuteAsync(query, new { Id = id });
        }
        public virtual Task AddAsync(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Added;
            return SaveAsync();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            return SaveAsync();
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return Db.Query<TEntity>($"select * from dbo.\"{TableName}\";");
        }

        public virtual void Add(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Added;
            Save();
        }

        public virtual TEntity Get(Guid id)
        {
            return Table.Find(id);
        }

        public virtual void Remove(Guid id)
        {
            var entity = Table.Find(id);
            Table.Remove(entity);
        }

        public int GetCount()
        {
            return Table.Count();
        }

        public Task<int> GetCountAsync()
        {
            return Table.CountAsync();
        }
        protected Task SaveAsync()
        {
            return DbContext.SaveChangesAsync();
        }
        protected void Save()
        {
            DbContext.SaveChanges();
        }

        //public virtual Task<TEntity> GetAsync(Guid id)
        //{
        //    return Db.QueryFirstAsync<TEntity>($"select * from dbo.\"{TableName}\" where \"Id\"=@id;", new { id });
        //}

        public TEntity Get(params object[] keyValues)
        {
            return Table.Find(keyValues);
        }

        public virtual Task RemoveAsync(TEntity entity)
        {
            Table.Remove(entity);
            return SaveAsync();
        }

        public virtual void Remove(TEntity entity)
        {
            Table.Remove(entity);
            Save();
        }

        public async virtual Task<TEntity> GetAsync(params object[] keyValues)
        {
            var entity = await ((DbSet<TEntity>)Table).FindAsync(keyValues);
            return entity;
        }
    }
}