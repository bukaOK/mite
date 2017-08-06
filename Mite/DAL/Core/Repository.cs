using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;
using System.Data.Entity;

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
            Db = dbContext.Database.Connection;
            DbContext = dbContext;
            Table = dbContext.Set<TEntity>();
            if(TableName == null)
                TableName = typeof(TEntity).Name + "s";
        }
        
        public virtual Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return Db.QueryAsync<TEntity>($"select * from dbo.{TableName}");
        }

        public virtual Task RemoveAsync(Guid id)
        {
            
            var query = $"delete from dbo.{TableName} where Id=@Id";
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

        public IEnumerable<TEntity> GetAll()
        {
            return Db.Query<TEntity>($"select * from dbo.{TableName}");
        }

        public void Add(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Added;
            Save();
        }

        public TEntity Get(Guid id)
        {
            return Db.QueryFirst<TEntity>($"select top 1 * from dbo.{TableName} where Id=@Id", new { Id = id });
        }

        public void Remove(Guid id)
        {
            var query = $"delete from dbo.{TableName} where Id=@Id";
            Db.Execute(query, new { Id = id });
        }

        public int GetCount()
        {
            var query = $"select COUNT(*) from dbo.{TableName}";
            return Db.QueryFirst<int>(query);
        }

        public Task<int> GetCountAsync()
        {
            var query = $"select COUNT(*) from dbo.{TableName}";
            return Db.QueryFirstAsync<int>(query);
        }
        protected Task SaveAsync()
        {
            return DbContext.SaveChangesAsync();
        }
        protected void Save()
        {
            DbContext.SaveChanges();
        }

        public Task<TEntity> GetAsync(Guid id)
        {
            return Db.QueryFirstAsync<TEntity>($"select * from dbo.{TableName} where Id=@id", new { id });
        }

        public TEntity Get(params object[] keyValues)
        {
            return Table.Find(keyValues);
        }

        public Task RemoveAsync(TEntity entity)
        {
            Table.Remove(entity);
            return SaveAsync();
        }

        public void Remove(TEntity entity)
        {
            Table.Remove(entity);
            Save();
        }

        public Task<TEntity> GetAsync(params object[] keyValues)
        {
            return ((DbSet<TEntity>)Table).FindAsync(keyValues);
        }
    }
}