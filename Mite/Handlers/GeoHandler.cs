using Mite.DAL.Entities;
using Mite.DAL.Repositories;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Handlers
{
    public class GeoHandler : HttpTaskAsyncHandler
    {
        readonly CitiesRepository citiesRepository;
        readonly CountriesRepository countriesRepository;

        public GeoHandler()
        {
            var dbContext = new DAL.Infrastructure.AppDbContext();
            citiesRepository = new CitiesRepository(dbContext);
            countriesRepository = new CountriesRepository(dbContext);
        }
        public async override Task ProcessRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var lat = req.QueryString["latitude"];
            var lon = req.QueryString["longitude"];

            Country country;
            City city;
            if(!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
            {
                city = await citiesRepository.GetNearliestAsync(double.Parse(lat), double.Parse(lon));
                country = await countriesRepository.GetAsync(city.CountryId);
            }
            else
            {
                country = await countriesRepository.GetByCodeAsync("RU");
                city = await citiesRepository.GetLargestByCountryAsync(country.Id);
            }
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonConvert.SerializeObject(new { countryId = country.Id, cityId = city.Id }));
        }
    }
}