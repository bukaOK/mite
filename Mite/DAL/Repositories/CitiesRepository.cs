using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Linq;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class CitiesRepository : Repository<City>
    {
        public bool CitiesInitialized => Table.Count() > 0;

        public CitiesRepository(AppDbContext dbContext) : base(dbContext)
        {
            TableName = "Cities";
        }
        public Task<IEnumerable<City>> GetAsync(string name)
        {
            var query = $"select * from dbo.\"{TableName}\" where \"Name\" like N'{name}%';";
            return Db.QueryAsync<City>(query);
        }
        public void AddRange(IEnumerable<City> cities)
        {
            foreach(var city in cities)
            {
                Table.Add(city);
            }
            Save();
        }
        public Task AddRangeAsync(IEnumerable<City> cities)
        {
            foreach (var city in cities)
            {
                Table.Add(city);
            }
            return SaveAsync();
        }
    }
}