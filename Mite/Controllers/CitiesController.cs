using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Mite.Controllers
{
    [Authorize(Roles = "moder,admin")]
    public class CitiesController : CrudApiController<City, CitiesRepository, City>
    {
        public CitiesController(IUnitOfWork unitOfWork, ILogger logger) : base(unitOfWork, logger)
        {
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<City>> GetByName(string name)
        {
            var cities = await Repo.GetAsync(name);
            return cities;
        }
    }
}