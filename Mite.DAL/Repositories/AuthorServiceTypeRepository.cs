using System.Collections.Generic;
using System.Threading.Tasks;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.DTO;
using Dapper;
using System;
using System.Linq;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class AuthorServiceTypeRepository : Repository<AuthorServiceType>
    {
        public AuthorServiceTypeRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task<IEnumerable<AuthorServiceTypeDTO>> GetAllWithPopularityAsync(bool confirmed)
        {
            var query = "select service_types.\"Id\", service_types.\"Name\", COUNT(services.\"ServiceTypeId\") as \"Popularity\" " +
                "from dbo.\"AuthorServiceTypes\" as service_types left outer join dbo.\"AuthorServices\" as services  on " +
                "services.\"ServiceTypeId\"=service_types.\"Id\" where service_types.\"Confirmed\"=@confirmed " +
                "group by service_types.\"Id\" order by \"Popularity\" desc";
            return Db.QueryAsync<AuthorServiceTypeDTO>(query, new { confirmed });
        }
        /// <summary>
        /// Кол-во услуг данного типа услуги
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetServiceCountAsync(Guid id)
        {
            return await Table.Where(x => x.Id == id).CountAsync();
        }
    }
}
