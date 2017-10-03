using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mite.SqlTable.Tables
{
    public class PsqlTable<TEntity>
    {
        private readonly string tableName;
        private readonly string schema;

        private string selectQuery = "";
        private string fromQuery = "";
        private string whereQuery = "";

        public PsqlTable( string tableName, string schema = "dbo")
        {
            this.tableName = tableName;
            this.schema = schema;
        }
        public PsqlTable<TResult> Select<TSource, TResult>(Expression<Func<TSource, TResult>> expr)
        {
            return null;
        }
    }
}
