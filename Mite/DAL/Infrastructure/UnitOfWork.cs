using System;
using Mite.DAL.Repositories;
using Mite.DAL.Core;

namespace Mite.DAL.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        
        public UnitOfWork()
        {
            dbContext = new AppDbContext();
        }
        public UnitOfWork(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public TRepo GetRepo<TRepo, TEntity>() where TRepo : IRepository<TEntity>
            where TEntity: class, new()
        {
            return (TRepo)Activator.CreateInstance(typeof(TRepo), dbContext);
        }
        public TRepo GetRepo<TRepo, TEntity>(params object[] additionalParams) where TRepo : IRepository<TEntity>
            where TEntity : class, new()
        {
            return (TRepo)Activator.CreateInstance(typeof(TRepo), dbContext, additionalParams);
        }
    }
}