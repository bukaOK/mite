using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Initializers.Core
{
    internal abstract class DBInitializer : IInitializer
    {
        public abstract bool Initialized { get; }
        protected AppDbContext DbContext { get; }

        public DBInitializer(AppDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public abstract void Initialize();
    }
}
