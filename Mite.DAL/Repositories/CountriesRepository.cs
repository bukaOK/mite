using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class CountriesRepository : Repository<Country>
    {
        public CountriesRepository(AppDbContext dbContext) : base(dbContext)
        {
            TableName = "Countries";
        }
        public Task<Country> GetByCodeAsync(string code)
        {
            return Table.FirstOrDefaultAsync(x => x.IsoCode == code);
        }
    }
}
