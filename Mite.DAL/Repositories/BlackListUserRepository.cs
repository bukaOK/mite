using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class BlackListUserRepository : Repository<BlackListUser>
    {
        public BlackListUserRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        /// <summary>
        /// Находится ли пользователь в черном списке
        /// </summary>
        /// <param name="targetId">Кого занесли</param>
        /// <param name="checkingUserId">Кто занес в черный список</param>
        /// <returns></returns>
        public async Task<bool> IsInBlackListAsync(string targetId, string checkingUserId)
        {
            return await DbContext.BlackListUsers.AnyAsync(x => x.CallerId == checkingUserId && x.ListedUserId == targetId);
        }
    }
}
