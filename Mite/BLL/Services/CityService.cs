using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Repositories;
using System;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.Models;
using AutoMapper;
using Mite.BLL.IdentityManagers;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Mite.BLL.Services
{
    public interface ICityService : ICrudService<CityModel, CitiesRepository, City>
    {
        Task<DataServiceResult> AddCityToUserAsync(Guid cityId, string userId);
        Task<CityModel> GetByNameAsync(string cityName);
        IEnumerable<SelectListItem> GetCitiesSelectList(User user);
        Task<IEnumerable<SelectListItem>> GetSelectListAsync(string userId);
    }
    public class CityService : CrudService<CityModel, CitiesRepository, City>, ICityService
    {
        private readonly AppUserManager userManager;

        public CityService(IUnitOfWork database, ILogger logger, AppUserManager userManager) : base(database, logger)
        {
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddCityToUserAsync(Guid cityId, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            user.CityId = cityId;
            try
            {
                await userManager.UpdateAsync(user);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при попытке привязать город к пользователю: " + e.Message);
                return DataServiceResult.Failed("Ошибка");
            }
        }

        public async Task<CityModel> GetByNameAsync(string cityName)
        {
            var city = await Repo.GetAsync(cityName);
            return Mapper.Map<CityModel>(city);
        }

        public IEnumerable<SelectListItem> GetCitiesSelectList(User user)
        {
            foreach(var city in Repo.GetAll())
            {
                var item = new SelectListItem
                {
                    Text = $"{city.Name}, {city.Region}",
                    Value = city.Id.ToString(),
                };
                if (city.Id == user.CityId)
                    item.Selected = true;
                yield return item;
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync(string userId)
        {
            var cities = await Repo.GetAllAsync();
            var user = string.IsNullOrEmpty(userId) ? null : await userManager.FindByIdAsync(userId);
            return cities.Select(city =>
            {
                var item = new SelectListItem
                {
                    Text = $"{city.Name}",
                    Value = city.Id.ToString(),
                };
                if (user != null && city.Id == user.CityId)
                {
                    item.Selected = true;
                }
                return item;
            });
        }
    }
}
