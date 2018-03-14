using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.Tests.DAL.Repositories
{
    [TestClass]
    public class OrdersRepositoryTest
    {
        OrderRepository repository;

        [TestInitialize]
        public void Init()
        {
            repository = new OrderRepository(new AppDbContext());
        }
        public async Task GetByFilterAsync()
        {
            var filter = new OrderTopFilter
            {
                Range = 30,
                Offset = 0
            };
            var result = await repository.GetByFilterAsync(filter);
            Assert.AreNotEqual(result.Count(), 0);

            filter.Input = "Не существующий";
            result = await repository.GetByFilterAsync(filter);
            Assert.AreEqual(result.Count(), 0);

            filter.Input = "разработать";
            result = await repository.GetByFilterAsync(filter);
            Assert.AreNotEqual(result.Count(), 0);
        }
    }
}
