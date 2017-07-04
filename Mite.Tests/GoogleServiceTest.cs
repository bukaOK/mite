using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Infrastructure;
using Mite.ExternalServices.Google;
using System.Net.Http;
using System.Threading.Tasks;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;

namespace Mite.Tests
{
    /// <summary>
    /// Summary description for GoogleServiceTest
    /// </summary>
    [TestClass]
    public class GoogleServiceTest
    {
        public GoogleServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        

        [TestMethod]
        public async Task RefreshTokenTestAsync()
        {
            var unitOfWork = new UnitOfWork(TestContext.DataConnection);
            var googleService = new GoogleService(unitOfWork, new HttpClient());
            var userManager = new AppUserManager(new UserStore<User>())

            var refreshToken = await googleService.GetRefreshTokenAsync()
        }
    }
}
