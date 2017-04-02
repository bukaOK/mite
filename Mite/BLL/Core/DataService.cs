using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Mite.BLL.Core
{
    public abstract class DataService
    {
        protected readonly IUnitOfWork Database;

        protected DataService(IUnitOfWork database)
        {
            Database = database;
        }
    }
}