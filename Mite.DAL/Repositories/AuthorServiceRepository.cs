using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using Mite.CodeData.Enums;
using Mite.DAL.Filters;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class AuthorServiceRepository : Repository<AuthorService>
    {
        public AuthorServiceRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task<AuthorService> GetWithServiceTypeAsync(Guid id)
        {
            return Table.Include(x => x.ServiceType).Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<AuthorService>> GetByUserAsync(string userId)
        {
            var services = await Table.Where(x => x.AuthorId == userId).Include(x => x.ServiceType).ToListAsync();
            return services;
        }
        public async Task<(double min, double max)> GetMinMaxPricesAsync()
        {
            var max = await Table.Where(x => x.Price != null).MaxAsync(x => x.Price);
            var min = await Table.Where(x => x.Price != null).MinAsync(x => x.Price);
            return (min: min ?? 0.0, max: max ?? 0.0);
        }
        /// <summary>
        /// Получить надежность
        /// </summary>
        /// <param name="id">Id услуги</param>
        /// <param name="badCoef">На сколько умножаем "плохие" статусы</param>
        /// <param name="goodCoef">На сколько умножаем "хорошие" для надежности статусы</param>
        /// <returns></returns>
        public async Task<int> GetReliabilityAsync(Guid id, int badCoef, int goodCoef)
        {
            var badCount = await DbContext.Deals
                .Where(x => x.ServiceId == id && (x.Status == DealStatuses.ModerRejected || x.Status == DealStatuses.ModerConfirmed))
                .CountAsync();
            var goodCount = await DbContext.Deals.Where(x => x.ServiceId == id && x.Status == DealStatuses.Confirmed)
                .CountAsync();
            return badCount * badCoef + goodCount * goodCoef;
        }
        public Task<IEnumerable<AuthorService>> GetByFilterAsync(ServiceTopFilter filter)
        {
            var query = "select * from dbo.\"AuthorServices\" as services inner join dbo.\"AuthorServiceTypes\" as service_types " +
                "on services.\"ServiceTypeId\"=service_types.\"Id\" inner join dbo.\"Users\" as users on services.\"AuthorId\"=users.\"Id\" " +
                "where services.\"CreateDate\" < @MaxDate ";
            if (filter.CityId != null)
                query += "and users.\"CityId\"=@CityId ";
            if (filter.Min != null)
                query += "and services.\"Price\" >= @Min ";
            if (filter.Max != null)
                query += "and services.\"Price\" <= @Max ";
            if (!string.IsNullOrEmpty(filter.Input))
                query += "and to_tsvector('mite_ru', services.\"Title\") @@ plainto_tsquery(@Input) ";


            switch (filter.SortFilter)
            {
                case ServiceSortFilter.Popular:
                    query += "order by services.\"Rating\" desc ";
                    break;
                case ServiceSortFilter.Reliable:
                    query += "order by services.\"Reliability\" desc ";
                    break;
            }
            query += "limit @Range offset @Offset;";


            return Db.QueryAsync<AuthorService, AuthorServiceType, User, AuthorService>(query, (service, serviceType, user) =>
                {
                    service.Author = user;
                    service.ServiceType = serviceType;
                    return service;
                }, filter);
        }
    }
}
