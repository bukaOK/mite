using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.Tests.DAL.Repositories
{
    [TestClass]
    public class ClientTariffTest
    {
        private AppDbContext dbContext;
        private ClientTariffRepository repository;

        [TestInitialize]
        public void Init()
        {
            dbContext = new AppDbContext();
            repository = new ClientTariffRepository(dbContext);
        }

        public async Task GetSponsors_ClientTariff()
        {
            var userId = "d6832baa-33d5-403d-b030-556acded1d69";
            var result = await repository.GetSponsorsAsync(userId, CodeData.Enums.SortFilter.New);
            Assert.IsTrue(result != null && result.Count() > 0);
        }
    }
}
