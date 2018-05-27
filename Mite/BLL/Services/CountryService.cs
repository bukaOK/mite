using Mite.BLL.Core;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.BLL.Services
{
    public interface ICountryService : IDataService
    {
        Task<IEnumerable<SelectListItem>> GetSelectListAsync(string userId);
    }
    public class CountryService : DataService, ICountryService
    {
        readonly CountriesRepository countriesRepository;
        private readonly AppUserManager userManager;

        public CountryService(IUnitOfWork database, ILogger logger, AppUserManager userManager) : base(database, logger)
        {
            countriesRepository = database.GetRepo<CountriesRepository, Country>();
            this.userManager = userManager;
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync(string userId)
        {
            var countries = await countriesRepository.GetAllAsync();
            var countryId = (Guid?)null;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                var userCity = await Database.GetRepo<CitiesRepository, City>().GetAsync(user.CityId);
                countryId = userCity?.CountryId;
            }

            return countries.Select(country => new SelectListItem
            {
                Text = country.Name,
                Value = country.Id.ToString(),
                Selected = country.Id == countryId
            });
        }
    }
}