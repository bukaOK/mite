using Mite.DAL.Core;
using Mite.DAL.Repositories;

namespace Mite.DAL.Infrastructure
{
    public interface IUnitOfWork
    {
        TRepo GetRepo<TRepo, TEntity>() where TRepo : IRepository<TEntity> where TEntity : class, new();
        TRepo GetRepo<TRepo, TEntity>(params object[] additionalParams) where TRepo : IRepository<TEntity> where TEntity : class, new();
    }
}