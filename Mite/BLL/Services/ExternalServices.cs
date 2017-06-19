using Mite.BLL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Mite.DAL.Infrastructure;
using NLog;

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
            var existingService = await Database.ExternalServiceRepository.GetAsync(userId, serviceType);
            if (existingService == null)
            {
                var service = new ExternalService
                {
                    AccessToken = accessToken,
                    Name = serviceType,
                    UserId = userId
                };
                await Database.ExternalServiceRepository.AddAsync(service);
            }
            else
            {
                existingService.AccessToken = accessToken;
                await Database.ExternalServiceRepository.UpdateAsync(existingService);
            }
        }

        public Task<ExternalService> Get(string userId, string serviceName)
        {
            return Database.ExternalServiceRepository.GetAsync(userId, serviceName);
        }

        public Task RemoveAsync(string userId, string serviceName)
        {
            return Database.ExternalServiceRepository.RemoveAsync(userId, serviceName);
        }

        public async Task<DataServiceResult> Update(string providerKey, string serviceType, string accessToken)
        {
            var existingService = await Database.ExternalServiceRepository.GetByProviderAsync(providerKey, serviceType);
            if(existingService == null)
            {
                logger.Error("Не найден внешний сервис");
                return DataServiceResult.Failed("Не найден внешний сервис");
            }
            existingService.AccessToken = accessToken;
            await Database.ExternalServiceRepository.UpdateAsync(existingService);
            return DataServiceResult.Success();
        }
    }
}