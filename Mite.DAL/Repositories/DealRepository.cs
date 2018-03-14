using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using Mite.CodeData.Enums;
using System.Data.Entity;
using Mite.DAL.DTO;

namespace Mite.DAL.Repositories
{
    public class DealRepository : Repository<Deal>
    {
        public DealRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<IEnumerable<Deal>> GetIncomingAsync(DealStatuses status, string authorId)
        {
            var deals = await Table.AsNoTracking()
                .Where(x => x.AuthorId == authorId && x.Status == status)
                .Include(x => x.Service).Include(x => x.Order)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return deals;
        }
        public async Task RateAsync(Deal deal, byte newVal)
        {
            var setVal = newVal - deal.Rating;
            deal.Rating = newVal;
            DbContext.Entry(deal).Property(x => x.Rating).IsModified = true;

            var service = await DbContext.AuthorServices.FirstOrDefaultAsync(x => x.Id == deal.ServiceId);
            if(service != null)
            {
                service.Rating += setVal;
                DbContext.Entry(service).Property(x => x.Rating).IsModified = true;

                var author = await DbContext.Users.FirstAsync(x => x.Id == service.AuthorId);
                author.Rating += setVal;
                DbContext.Entry(author).Property(x => x.Rating).IsModified = true;
            }
            else
            {
                var order = await DbContext.Orders.FirstOrDefaultAsync(x => x.Id == deal.OrderId);
                var executer = await DbContext.Users.FirstAsync(x => x.Id == order.ExecuterId);
                executer.Rating += setVal;
                DbContext.Entry(executer).Property(x => x.Rating).IsModified = true;
            }
            await SaveAsync();
        }
        public async Task<IEnumerable<Deal>> GetOutgoingAsync(DealStatuses status, string clientId)
        {
            var deals = await Table.AsNoTracking()
                .Where(x => x.ClientId == clientId && x.Status == status)
                .Include(x => x.Service).Include(x => x.Order)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
            return deals;
        }
        public async Task<IList<Deal>> GetForModerAsync(DealStatuses status, string moderId)
        {
            var query = Table.AsNoTracking()
                .Where(x => x.Status == status && x.ClientId != moderId && x.AuthorId != moderId);
            switch (status)
            {
                case DealStatuses.Dispute:
                    if (string.IsNullOrEmpty(moderId))
                        query = query.Where(x => x.ModerId == null);
                    else
                        query = query.Where(x => x.ModerId == moderId);
                    break;
                default:
                    query = query.Where(x => x.ModerId == moderId);
                    break;
            }
            return await query.Include(x => x.Service)
                .Include(x => x.Order)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();
        }
        public Task<Deal> GetWithServiceAsync(long id)
        {
            return Table.AsNoTracking().Include(x => x.Service).Include(x => x.Order)
                .Include(x => x.Author).Include(x => x.DisputeChat.Members).Include(x => x.Moder)
                .Include(x => x.Client).Include(x => x.Chat.Members).FirstOrDefaultAsync(x => x.Id == id);
        }
        public int GetAuthorCounts(string authorId)
        {
            return Table.Count(x => x.AuthorId == authorId && (x.Status == DealStatuses.New || 
                x.Status == DealStatuses.ExpectClient || x.Status == DealStatuses.Dispute));
        }
        public int GetClientCounts(string clientId)
        {
            return Table.Count(x => x.ClientId == clientId && (x.Status == DealStatuses.ExpectPayment || x.Status == DealStatuses.ExpectClient
                || x.Status == DealStatuses.New || x.Status == DealStatuses.Dispute));
        }
        /// <summary>
        /// Получить список изображений результатов работ
        /// </summary>
        /// <param name="serviceId">Id услуги</param>
        /// <returns></returns>
        public async Task<IList<DealGalleryItemDTO>> GetServiceGalleryAsync(Guid serviceId)
        {
            return await Table.Where(x => x.ServiceId == serviceId && x.ImageResultSrc != null)
                .Select(x => new DealGalleryItemDTO
                {
                    ImageSrc = x.ImageResultSrc,
                    ImageSrcCompressed = x.ImageResultSrc_50,                    
                }).ToListAsync();
        }
    }
}
