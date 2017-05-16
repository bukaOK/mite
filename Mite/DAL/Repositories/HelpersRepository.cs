using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class HelpersRepository : Repository<Helper>
    {
        public HelpersRepository(IDbConnection db) : base(db)
        {
        }
        public Task<Helper> GetByUserAsync(string userId)
        {
            var query = "select * from dbo.Helpers where UserId=@userId";
            return Db.QueryFirstOrDefaultAsync<Helper>(query, new { userId });
        }
        public Helper GetByUser(string userId)
        {
            var query = "select * from dbo.Helpers where UserId=@userId";
            return Db.QueryFirstOrDefault<Helper>(query, new { userId });
        }
    }
}