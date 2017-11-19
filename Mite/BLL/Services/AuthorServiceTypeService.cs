using Mite.BLL.Core;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.DAL.Entities;
using System;
using Mite.DAL.Repositories;
using NLog;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Mite.Models;
using AutoMapper;

namespace Mite.BLL.Services
{
    public interface IAuthorServiceTypeService : IDataService
    {
        Task<DataServiceResult> AddAsync(ServiceTypeModel model);
        Task<DataServiceResult> UpdateAsync(ServiceTypeModel model);
        Task<DataServiceResult> RemoveAsync(Guid id);
        Task<IEnumerable<SelectListItem>> GetSelectListAsync(Guid selectedServiceTypeId);
        Task<IEnumerable<AuthorServiceType>> GetAllAsync();
    }
    public class AuthorServiceTypeService : DataService, IAuthorServiceTypeService
    {
        private readonly AuthorServiceTypeRepository repo;

        public AuthorServiceTypeService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            repo = Database.GetRepo<AuthorServiceTypeRepository, AuthorServiceType>();
        }

        public async Task<DataServiceResult> AddAsync(ServiceTypeModel model)
        {
            var serviceType = Mapper.Map<AuthorServiceType>(model);
            try
            {
                await repo.AddAsync(serviceType);
                model.Id = serviceType.Id;
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при добавлении типа услуги: " + e.Message);
                return DataServiceResult.Failed("Ошибка при добавлении типа услуги");
            }
        }

        public async Task<DataServiceResult> UpdateAsync(ServiceTypeModel model)
        {
            var serviceType = await repo.GetAsync(model.Id);
            if (serviceType == null)
                return DataServiceResult.Failed("Не найден тип услуги.");
            Mapper.Map(model, serviceType);
            try
            {
                await repo.UpdateAsync(serviceType);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при подтверждении типа услуги: " + e.Message);
                return DataServiceResult.Failed("Ошибка при подтверждении типа услуги");
            }
        }

        public Task<IEnumerable<AuthorServiceType>> GetAllAsync()
        {
            return repo.GetAllAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetSelectListAsync(Guid selectedServiceTypeId)
        {
            var serviceTypes = await repo.GetAllWithPopularityAsync(true);
            return serviceTypes.Select(serviceType =>
            {
                var item = new SelectListItem
                {
                    Text = serviceType.Name,
                    Value = serviceType.Id.ToString()
                };
                if (selectedServiceTypeId == serviceType.Id)
                    item.Selected = true;

                return item;
            });
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id)
        {
            var entity = await repo.GetAsync(id);
            if (entity == null)
                return DataServiceResult.Failed("Не найден тип услуги.");
            try
            {
                await repo.RemoveAsync(entity);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при удалении типа услуги: " + e.Message);
                return DataServiceResult.Failed("Ошибка при удалении типа услуги");
            }
        }
    }
}