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
using System.Collections;
using Mite.DAL.DTO;

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
        public async Task<IList<FeedbackDTO>> GetFeedbacksAsync(Guid serviceId)
        {
            var feedbacks = await DbContext.Deals.Include(x => x.Client)
                .Where(x => x.ServiceId == serviceId && x.Feedback != null)
                .Select(x => new FeedbackDTO
                {
                    Content = x.Feedback,
                    User = x.Client
                }).ToListAsync();
            return feedbacks;
        }
        public async Task<int> DealsCountAsync(Guid serviceId)
        {
            var count = await DbContext.Deals.Where(x => x.ServiceId == serviceId).CountAsync();
            return count;
        }
        public Task<IEnumerable<AuthorService>> GetByUserAsync(string userId, SortFilter sort)
        {
            var query = "select * from dbo.\"AuthorServices\" as services inner join dbo.\"AuthorServiceTypes\" as service_types " +
                "on services.\"ServiceTypeId\"=service_types.\"Id\" where services.\"AuthorId\"=@userId order by ";
            switch (sort)
            {
                case SortFilter.New:
                    query += "services.\"CreateDate\" desc";
                    break;
                case SortFilter.Old:
                    query += "services.\"CreateDate\" asc";
                    break;
                case SortFilter.Popular:
                default:
                    query += "services.\"Rating\" desc";
                    break;
            }
            query += ";";
            return Db.QueryAsync<AuthorService, AuthorServiceType, AuthorService>(query, (service, serviceType) =>
            {
                service.ServiceType = serviceType;
                return service;
            }, new { userId });
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
        public async Task RecountReliabilityAsync(Guid id, int badCoef, int goodCoef)
        {
            var query = $"select count(*) from dbo.\"Deals\" where \"ServiceId\"=@id and \"Status\"={(int)DealStatuses.Confirmed};";
            var qParams = new { id };
            var goodCount = await Db.QueryFirstAsync<int>(query, qParams);
            query = "select count(*) from dbo.\"Deals\" where \"ServiceId\"=@id " +
                $"and (\"Status\"={(int)DealStatuses.ModerConfirmed} or \"Status\"={(int)DealStatuses.ModerRejected});";
            var badCount = await Db.QueryFirstAsync<int>(query, qParams);
            var reliability = badCount * badCoef + goodCount * goodCoef;

            var service = await Table.FirstAsync(x => x.Id == id);
            if(service.Reliability != reliability)
            {
                service.Reliability = reliability;
                DbContext.Entry(service).Property(x => x.Reliability).IsModified = true;
                await SaveAsync();
            }
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
            if (filter.ServiceTypeId != null)
                query += "and service_types.\"Id\"=@ServiceTypeId ";
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
