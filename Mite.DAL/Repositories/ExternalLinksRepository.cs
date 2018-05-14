using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ExternalLinksRepository : Repository<ExternalLink>
    {
        public ExternalLinksRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ExternalLink> GetByUrlAsync(string url)
        {
            return await Table.FirstOrDefaultAsync(x => x.Url == url);
        }
        public async Task<IEnumerable<ExternalLink>> GetByUserAsync(string userId)
        {
            var links = await Table.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
            return links;
        }
        public async Task AddOrUpdateAsync(IEnumerable<ExternalLink> links)
        {
            var userId = links.FirstOrDefault().UserId;
            var existingLinks = await Table.Where(x => x.UserId == userId).ToListAsync();

            var toUpdate = existingLinks.Where(x => links.Any(y => x.Id == y.Id));
            var toAdd = links.Where(x => !existingLinks.Any(y => y.Id == x.Id));
            var toDel = existingLinks.Except(toUpdate);

            foreach(var link in toUpdate)
            {
                var newLink = links.FirstOrDefault(x => x.Id == link.Id);
                if(newLink.Url != link.Url)
                {
                    link.Url = newLink.Url;
                    DbContext.Entry(link).Property(x => x.Url).IsModified = true;
                }
            }
            if(toAdd.Count() > 0)
                Table.AddRange(toAdd);
            if(toDel.Count() > 0)
                Table.RemoveRange(toDel);
            await SaveAsync();
        }
    }
}
