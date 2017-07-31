using Mite.BLL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.DAL.Repositories;

namespace Mite.BLL.Services
{
    public interface IExternalServices
    {
        /// <summary>
        /// Добавляет внешний сервис пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceType">Тип сервиса(Google, Facebook, etc.)</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task Add(string userId, string serviceType, string accessToken);
        Task<ExternalService> Get(string userId, string serviceName);
        /// <summary>
        /// Обновляет сервис
        /// </summary>
        /// <param name="providerKey">Id пользователя во внешнем сервисе</param>
        /// <param name="serviceType"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<DataServiceResult> Update(string providerKey, string serviceType, string accessToken);
        Task RemoveAsync(string userId, string serviceName);
    }
    public class ExternalServices : DataService, IExternalServices
    {
        private readonly ILogger logger;

        public ExternalServices(IUnitOfWork database, ILogger logger) : base(database)
        {
            this.logger = logger;
        }

        public async Task Add(string userId, string serviceType, string accessToken)
        {
            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var existingService = await repo.GetAsync(userId, serviceType);
            if (existingService == null)
            {
                var service = new ExternalService
                {
                    AccessToken = accessToken,
                    Name = serviceType,
                    UserId = userId
                };
                await repo.AddAsync(service);
            }
            else
            {
                existingService.AccessToken = accessToken;
                await repo.UpdateAsync(existingService);
            }
        }

        public Task<ExternalService> Get(string userId, string serviceName)
        {
            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            return repo.GetAsync(userId, serviceName);
        }

        public Task RemoveAsync(string userId, string serviceName)
        {
            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            return repo.RemoveAsync(userId, serviceName);
        }

        public async Task<DataServiceResult> Update(string providerKey, string serviceType, string accessToken)
        {
            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var existingService = await repo.GetByProviderAsync(providerKey, serviceType);
            if(existingService == null)
            {
                logger.Error("Не найден внешний сервис");
                return DataServiceResult.Failed("Не найден внешний сервис");
            }
            existingService.AccessToken = accessToken;
            await repo.UpdateAsync(existingService);
            return DataServiceResult.Success();
        }
    }
}