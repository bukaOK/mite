using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mite.Tests.DAL.Repositories
{
    [TestClass]
    public class ProductsRepoTest
    {
        private ProductsRepository productsRepository;
        private Guid productId;

        [TestInitialize]
        public void Init()
        {
            var dbContext = new AppDbContext();
            productsRepository = new ProductsRepository(dbContext);
            productId = new Guid("{FF1EF56A-FA3C-471E-B6C2-2B9951FE1318}");
        }
        
    }
}
