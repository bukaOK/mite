using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Repositories;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Mite.Tests.DAL.Repositories
{
    [TestClass]
    public class PostsRepoTest
    {
        private const string ConnectionString = @"Data Source=194.87.103.114\SQLEXPRESS,1433;Initial Catalog=MiteDb;User ID=Buka;Password=Evd$utTC";
        private readonly PostsRepository repo = new PostsRepository(new SqlConnection(ConnectionString));

        [TestMethod]
        public async Task GetByFilterTest()
        {
            var minDate = new DateTime(1800, 1, 1);
            var page = 1;
            const int range = 9;
            var offset = (page - 1) * range;

            var page1 = await repo.GetByFilterAsync(minDate, true, "46d46e53-3035-4772-b8cd-99db788eaaf0", Enums.SortFilter.New, offset, range);
            page = 2;
            offset = (page - 1) * range;
            var page2 = await repo.GetByFilterAsync(minDate, true, "46d46e53-3035-4772-b8cd-99db788eaaf0", Enums.SortFilter.New, offset, range);
            page = 3;
            offset = (page - 1) * range;    
            var page3 = await repo.GetByFilterAsync(minDate, true, "46d46e53-3035-4772-b8cd-99db788eaaf0", Enums.SortFilter.New, offset, range);
        }
    }
}
