using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ESPublishSettingsRepository : Repository<ESPublishSetting>
    {
        public ESPublishSettingsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public Task<ESPublishSetting> GetByUserAsync(string userId)
        {
            return Table.FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
