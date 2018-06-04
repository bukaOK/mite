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
        public async Task<DailyFact> GetDailyFactAsync()
        {
            var now = DateTime.UtcNow;
            //Находим факт, который определен на сегодня
            var fact = await Table.FirstOrDefaultAsync(x => x.StartDate <= now && x.EndDate >= now);
            if (fact != null)
                return fact;
            //Если такого нет, находим факт без даты
            fact = await Table.FirstOrDefaultAsync(x => x.StartDate == null && x.EndDate == null);
            if (fact == null)
                return null;
            //Задаем дату на сегодня
            fact.StartDate = new DateTime(now.Year, now.Month, now.Day);
            fact.EndDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            DbContext.Entry(fact).State = EntityState.Modified;
            Save();

            return fact;
        }
        public Task ResetFactDatesAsync()
        {
            var query = "update dbo.\"DailyFacts\" set \"StartDate\"=null, \"EndDate\"=null;";
            return Db.ExecuteAsync(query);
        }
    }
}
