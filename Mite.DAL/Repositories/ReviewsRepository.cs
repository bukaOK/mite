using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
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
    }
}
