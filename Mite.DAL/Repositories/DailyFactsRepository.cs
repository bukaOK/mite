using Dapper;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class DailyFactsRepository : Repository<DailyFact>
    {
        public DailyFactsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public DailyFact GetDailyFact()
        {
            var now = DateTime.UtcNow;
            var query = "select * from dbo.\"DailyFacts\" where \"StartDate\"<=@now and \"EndDate\">=@now limit 1;";
            var fact = Db.QueryFirstOrDefault<DailyFact>(query, new { now });
            if (fact != null)
                return fact;
            query = "select * from (select * from dbo.\"DailyFacts\" where \"StartDate\" is null or \"EndDate\" is null) " +
                "tablesample system_rows(1);";
            fact = Db.QueryFirstOrDefault<DailyFact>(query);
            if (fact == null)
                return null;

            fact.StartDate = new DateTime(now.Year, now.Month, now.Day);
            fact.EndDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            DbContext.Entry(fact).State = EntityState.Modified;
            Save();

            return fact;
        }
        public Task ResetFactDates()
        {
            var query = "update dbo.\"DailyFacts\" set \"StartDate\"=null, \"EndDate\"=null;";
            return Db.ExecuteAsync(query);
        }
    }
}
