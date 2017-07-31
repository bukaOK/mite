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
        public TRepo GetRepo<TRepo, TEntity>() where TRepo : Repository<TEntity>
            where TEntity: class, new()
        {
            return Activator.CreateInstance(typeof(TRepo), dbContext) as TRepo;
        }
        public TRepo GetRepo<TRepo, TEntity>(params object[] additionalParams) where TRepo : Repository<TEntity>
            where TEntity : class, new()
        {
            return Activator.CreateInstance(typeof(TRepo), dbContext, additionalParams) as TRepo;
        }
    }
}