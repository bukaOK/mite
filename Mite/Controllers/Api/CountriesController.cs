using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    public class CountriesController : ApiController
    {
        readonly CountriesRepository countriesRepository;

        public CountriesController(IUnitOfWork unitOfWork)
        {
            countriesRepository = unitOfWork.GetRepo<CountriesRepository, Country>();
        }

        public async Task<IEnumerable<Country>> GetAll()
        {
            return await countriesRepository.GetAllAsync();
        }
    }
}