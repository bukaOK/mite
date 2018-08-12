using AutoMapper;
using Mite.BLL.Core;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.BLL.Services
{
    public interface ITariffService : IDataService
    {
        Task<TariffModel> GetAsync(Guid tariffId);
        Task<DataServiceResult> AddAsync(TariffModel model);
        Task<DataServiceResult> UpdateAsync(TariffModel model);
        /// <summary>
        /// Получить тарифы для автора или для клиента
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="forAuthor"></param>
        /// <returns></returns>
        Task<IEnumerable<TariffModel>> GetForAuthorAsync(string userId);
        /// <summary>
        /// Получить список тарифов определенного автора для клиента
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="authorName"></param>
        /// <returns></returns>
        Task<ClientTariffsModel> GetForClientAsync(string clientId, string authorName);
        /// <summary>
        /// Получить платные подписки клиента
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<IEnumerable<TariffModel>> GetForClientAsync(string clientId);
        /// <summary>
        /// Добавить тариф в список тарифов клиента
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddForClientAsync(Guid tariffId, string clientId);
        /// <summary>
        /// Удалить тариф у клиента
        /// </summary>
        /// <param name="tariffId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<DataServiceResult> RemoveForClientAsync(Guid tariffId, string clientId);
        /// <summary>
        /// Удалить тариф 
        /// </summary>
        /// <param name="tariffId"></param>
        /// <returns></returns>
        Task<DataServiceResult> RemoveAsync(Guid tariffId);
        Task<DataServiceResult> TariffCheckoutAsync(Guid tariffId, string clientId);
        Task<IEnumerable<SelectListItem>> GetForPostAsync(Guid? tariffId, string authorId);
        /// <summary>
        /// Получить спонсоров пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<UserShortModel>> GetSponsorsAsync(string userId, SortFilter sort);
    }
    public class TariffService : DataService, ITariffService
    {
        private readonly TariffRepository repository;
        private readonly ClientTariffRepository clientRepository;
        private readonly ICashService cashService;
        private readonly AppUserManager userManager;

        public TariffService(IUnitOfWork database, ILogger logger, ICashService cashService, AppUserManager userManager) : base(database, logger)
        {
            repository = database.GetRepo<TariffRepository, AuthorTariff>();
            clientRepository = database.GetRepo<ClientTariffRepository, ClientTariff>();
            this.cashService = cashService;
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddAsync(TariffModel model)
        {
            try
            {
                var entity = Mapper.Map<AuthorTariff>(model);
                await repository.AddAsync(entity);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении тарифа", e);
            }
        }

        public async Task<DataServiceResult> AddForClientAsync(Guid tariffId, string clientId)
        {
            using(var trans = clientRepository.BeginTransaction())
            {
                try
                {
                    var clientCash = await cashService.GetUserCashAsync(clientId);
                    var tariff = await repository.GetAsync(tariffId);
                    if (tariff.Price > clientCash)
                        return DataServiceResult.Failed("Недостаточно средств");

                    var existingTariff = await clientRepository.GetAsync(tariff.Id, clientId);
                    if (existingTariff != null && existingTariff.Tariff.Id == tariff.Id)
                        return DataServiceResult.Failed("Вы уже подписаны на этот тариф");

                    await cashService.AddAsync(clientId, tariff.AuthorId, tariff.Price, CashOperationTypes.TariffPay);
                    await clientRepository.RemoveAuthorTariffsAsync(clientId, tariff.AuthorId);
                    await clientRepository.AddAsync(new ClientTariff
                    {
                        TariffId = tariffId,
                        ClientId = clientId,
                        LastPayTimeUtc = DateTime.UtcNow,
                        PayStatus = TariffStatuses.Paid
                    });
                    trans.Commit();
                    return Success;
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    return CommonError("Ошибка при оформлении платной подписки", e);
                }
            }
        }

        public async Task<TariffModel> GetAsync(Guid tariffId)
        {
            var tariff = await repository.GetAsync(tariffId);
            return Mapper.Map<TariffModel>(tariff);
        }

        public async Task<IEnumerable<TariffModel>> GetForAuthorAsync(string userId)
        {
            var tariffs = await repository.GetByAuthorAsync(userId);
            return Mapper.Map<IEnumerable<TariffModel>>(tariffs);
        }

        public async Task<ClientTariffsModel> GetForClientAsync(string clientId, string authorName)
        {
            var author = await userManager.FindByNameAsync(authorName);
            var tariffs = await repository.GetByAuthorAsync(author.Id);
            var selectedTariff = await clientRepository.GetByClientFirstAsync(clientId, author.Id);

            return new ClientTariffsModel
            {
                Author = Mapper.Map<UserShortModel>(author),
                Tariffs = Mapper.Map<List<TariffModel>>(tariffs),
                SelectedTariff = Mapper.Map<TariffModel>(selectedTariff?.Tariff)
            };
        }

        public async Task<IEnumerable<TariffModel>> GetForClientAsync(string clientId)
        {
            var tariffs = await clientRepository.GetByClientAsync(clientId);
            return Mapper.Map<IEnumerable<TariffModel>>(tariffs.Select(x => x.Tariff));
        }

        public async Task<IEnumerable<SelectListItem>> GetForPostAsync(Guid? tariffId, string authorId)
        {
            var tariffs = await repository.GetByAuthorAsync(authorId);

            return tariffs.Select(x => new SelectListItem
            {
                Text = x.Title,
                Value = x.Id.ToString(),
                Selected = tariffId == x.Id
            });
        }

        public async Task<IEnumerable<UserShortModel>> GetSponsorsAsync(string userId, SortFilter sort)
        {
            var result = await clientRepository.GetSponsorsAsync(userId, sort);
            return Mapper.Map<IEnumerable<UserShortModel>>(result);
        }

        public async Task<DataServiceResult> RemoveAsync(Guid tariffId)
        {
            var hasAnyClients = await repository.HasAnyClientsAsync(tariffId);
            if (hasAnyClients)
                return DataServiceResult.Failed("У тарифа есть подписчики, рекомендуем изменить цену или условия");
            try
            {
                await repository.RemoveAsync(tariffId);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении тарифа", e);
            }
        }

        public async Task<DataServiceResult> RemoveForClientAsync(Guid tariffId, string clientId)
        {
            try
            {
                var clTariff = await clientRepository.GetAsync(tariffId, clientId);
                if (clTariff == null)
                    return DataServiceResult.Failed("У клиента не найден тариф");
                await clientRepository.RemoveAsync(clTariff);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении тарифа у клиента", e);
            }
        }

        public async Task<DataServiceResult> TariffCheckoutAsync(Guid tariffId, string clientId)
        {
            var clTariff = await clientRepository.GetAsync(tariffId, clientId);
            if (clTariff == null)
                return DataServiceResult.Failed("У клиента не найден тариф");
            try
            {
                if ((clTariff.LastPayTimeUtc - DateTime.UtcNow).Days >= 30)
                {
                    var cash = await cashService.GetUserCashAsync(clTariff.ClientId);
                    if (cash - clTariff.Tariff.Price < 0)
                    {
                        clTariff.PayStatus = TariffStatuses.NotPaid;
                    }
                    else
                    {
                        await cashService.AddAsync(clTariff.ClientId, clTariff.Tariff.AuthorId, clTariff.Tariff.Price, CashOperationTypes.TariffPay);
                        clTariff.PayStatus = TariffStatuses.Paid;
                    }
                    await clientRepository.UpdateAsync(clTariff);
                }
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при оформлении платной подписки", e);
            }
        }

        public async Task<DataServiceResult> UpdateAsync(TariffModel model)
        {
            try
            {
                var entity = await repository.GetAsync(model.Id);
                entity.Description = model.Description;
                entity.Title = model.Header;
                entity.Price = (double)model.Price;
                await repository.UpdateAsync(entity);
                return Success;
            }
            catch (Exception e)
            {
                return CommonError("Ошибка при добавлении тарифа", e);
            }
        }

    }
}