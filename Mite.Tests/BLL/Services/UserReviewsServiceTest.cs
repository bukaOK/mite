using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.BLL.Services;
using Mite.DAL.Infrastructure;
using NLog;
using System.Threading.Tasks;

namespace Mite.Tests.BLL.Services
{
    [TestClass]
    public class UserReviewsServiceTest
    {
        private IUnitOfWork unitOfWork;
        private IUserReviewService reviewService;
        private const string UserId = "d6832baa-33d5-403d-b030-556acded1d69";

        [TestInitialize]
        public void Init()
        {
            unitOfWork = new UnitOfWork(new AppDbContext());
            
            reviewService = new UserReviewService(unitOfWork, LogManager.GetLogger("LOGGER"));
        }
        [TestMethod]
        public async Task AddAsyncTest()
        {
            await reviewService.AddAsync(UserId, "Нормалек вроде");
        }
    }
}
