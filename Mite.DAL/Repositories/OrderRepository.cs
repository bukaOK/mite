using Dapper;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.DTO;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class OrderRepository : Repository<Order>
    {
        public OrderRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<Order> GetAsync(Guid id)
        {
            return await Table.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<OrderTopDTO>> GetByFilterAsync(OrderTopFilter filter)
        {
            var query = "select order_counts.*, \"UserName\", tps.* from " +
                "(select orders.*, count(requests.\"OrderId\") as \"RequestsCount\" from dbo.\"Orders\" orders " +
                "left outer join dbo.\"OrderRequests\" requests on requests.\"OrderId\"=orders.\"Id\" " +
                "group by orders.\"Id\") order_counts inner join dbo.\"Users\" users on users.\"Id\"=order_counts.\"UserId\" " +
                "inner join dbo.\"AuthorServiceTypes\" tps on tps.\"Id\"=order_counts.\"OrderTypeId\" where order_counts.\"Status\"=@OpenStatus " +
                "and order_counts.\"CreateDate\" < @MaxDate ";
            if(!string.IsNullOrEmpty(filter.Input))
                query += "and to_tsvector('mite_ru', order_counts.\"Title\") @@ plainto_tsquery(@Input) ";
            if (filter.OrderTypeId != null)
                query += "and tps.\"Id\"=@OrderTypeId ";
            if (filter.CityId != null)
                query += "and users.\"CityId\"=@CityId ";
            if (filter.MaxPrice != null)
                query += "and order_counts.\"Price\" < @MaxPrice ";
            if (filter.MinPrice != null)
                query += "and order_counts.\"Price\" > @MinPrice ";

            query += "order by order_counts.\"CreateDate\" desc limit @Range offset @Offset;";
            return await Db.QueryAsync<OrderTopDTO, AuthorServiceType, OrderTopDTO>(query, (order, orderType) =>
            {
                order.OrderType = orderType;
                return order;
            }, filter);
        }
        public async Task<Order> GetWithUserAsync(Guid id)
        {
            return await Table.AsNoTracking().Include(x => x.OrderType).Include(x => x.User).FirstAsync(x => x.Id == id);
        }
        public Task<IEnumerable<OrderTopDTO>> GetByUserAsync(string userId, OrderStatuses status)
        {
            var query = "select order_counts.*, tps.* from " +
                "(select orders.*, count(requests.\"OrderId\") as \"RequestsCount\" from dbo.\"Orders\" orders " +
                "left outer join dbo.\"OrderRequests\" requests on requests.\"OrderId\"=orders.\"Id\" " +
                "group by orders.\"Id\") order_counts inner join dbo.\"AuthorServiceTypes\" tps on tps.\"Id\"=order_counts.\"OrderTypeId\" " +
                "where order_counts.\"Status\"=@status and order_counts.\"UserId\"=@userId order by order_counts.\"CreateDate\" desc;";
            return Db.QueryAsync<OrderTopDTO, AuthorServiceType, OrderTopDTO>(query, (order, orderType) =>
            {
                order.OrderType = orderType;
                return order;
            }, new { userId, status });
        }
    }
}
