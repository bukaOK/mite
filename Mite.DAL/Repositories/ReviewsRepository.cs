using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class ReviewsRepository : Repository<UserReview>
    {
        public ReviewsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public UserReview GetByUser(string userId)
        {
            return Table.FirstOrDefault(x => x.UserId == userId);
        }
        public override async Task<IEnumerable<UserReview>> GetAllAsync()
        {
            return await Table.AsNoTracking().Include(x => x.User).Where(x => x.Review != null).ToListAsync();
        }
        public override IEnumerable<UserReview> GetAll()
        {
            return Table.AsNoTracking().Include(x => x.User).Where(x => x.Review != null).ToList();
        }
    }
}
