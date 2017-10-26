using Mite.DAL.Core;
using Mite.DAL.Entities;
using System.Linq;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class CitiesRepository : Repository<City>
    {
        public bool CitiesInitialized => Table.Count() > 0;

        public CitiesRepository(AppDbContext dbContext) : base(dbContext)
        {
            TableName = "Cities";
        }
        public override async Task<IEnumerable<City>> GetAllAsync()
        {
            var query = "select * from dbo.\"Cities\" order by \"Population\" desc;";
            return await Db.QueryAsync<City>(query);
        }
        public override IEnumerable<City> GetAll()
        {
            var query = "select * from dbo.\"Cities\" order by \"Population\" desc;";
            return Db.Query<City>(query);
        }
        public Task<City> GetAsync(string name)
        {
            var query = $"select * from dbo.\"{TableName}\" where \"Name\" like N'{name}%';";
            return Db.QueryFirstOrDefaultAsync<City>(query);
        }
        public override Task UpdateAsync(City entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;

            if (entity.Latitude == null)
                DbContext.Entry(entity).Property(x => x.Latitude).IsModified = false;
            if (entity.Longitude == null)
                DbContext.Entry(entity).Property(x => x.Longitude).IsModified = false;
            return SaveAsync();
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