using AutoMapper;
using Mite.BLL.Core;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Mite.BLL.Services
{
    public interface IOrderService
    {
        Task<DataServiceResult> CreateAsync(OrderEditModel model);
        Task<DataServiceResult> UpdateAsync(OrderEditModel model);
        Task<DataServiceResult> RemoveAsync(Guid id);
        Task<OrderShowModel> GetAsync(Guid id, string currentUserId);
        /// <summary>
        /// Подать заявку на выполнение
        /// </summary>
        /// <param name="executerId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddRequestAsync(string executerId, Guid orderId);
        Task<DataServiceResult> RemoveRequestAsync(string executerId, Guid orderId);
        Task<DataServiceResult> ChoseExecuterAsync(string executerId, Guid orderId, string currentUserId);
        Task<OrderEditModel> GetToEditAsync(Guid id);
        Task<IEnumerable<OrderTopModel>> GetTopAsync(OrderTopFilterModel model);
        Task<IEnumerable<OrderTopModel>> GetByUserAsync(string userId, OrderStatuses status);
    }
    public class OrderService : DataService, IOrderService
    {
        private readonly OrderRepository repository;
        private readonly OrderRequestRepository requestRepository;
        private readonly IDealService dealService;

        public OrderService(IUnitOfWork database, ILogger logger, IDealService dealService) : base(database, logger)
        {
            repository = database.GetRepo<OrderRepository, Order>();
            requestRepository = database.GetRepo<OrderRequestRepository, OrderRequest>();
            this.dealService = dealService;
        }

        public async Task<DataServiceResult> AddRequestAsync(string executerId, Guid orderId)
        {
            var order = await repository.GetAsync(orderId);
            if (order.ExecuterId != null)
                return DataServiceResult.Failed("Исполнитель уже выбран");
            try
            {
                await requestRepository.AddAsync(new OrderRequest
                {
                    ExecuterId = executerId,
                    OrderId = orderId,
                    RequestDate = DateTime.UtcNow
                });
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при создании заявки", e);
            }
        }

        public async Task<DataServiceResult> ChoseExecuterAsync(string executerId, Guid orderId, string currentUserId)
        {
            var orderRequest = await requestRepository.GetAsync(executerId, orderId);
            if (orderRequest == null)
                return DataServiceResult.Failed("Заявка не найдена");
            var order = await repository.GetAsync(orderId);
            if (order.Status != OrderStatuses.Open)
                return DataServiceResult.Failed("Исполнитель уже выбран");

            try
            {
                order.ExecuterId = executerId;
                order.Status = OrderStatuses.ExecuterChosed;
                await repository.UpdateAsync(order);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при выборе исполнителя", e);
            }
            return await dealService.CreateAsync(orderId, executerId, currentUserId);
        }

        public async Task<DataServiceResult> CreateAsync(OrderEditModel model)
        {
            try
            {
                var order = Mapper.Map<Order>(model);
                if (model.ImageStream != null)
                {
                    order.ImageSrc = FilesHelper.CreateImage(PathConstants.PublicVirtualImageFolder, model.ImageStream);
                }
                order.CreateDate = DateTime.UtcNow;
                await repository.AddAsync(order);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при создании заказа", e);
            }
        }

        public async Task<OrderShowModel> GetAsync(Guid id, string currentUserId)
        {
            var order = await repository.GetWithUserAsync(id);
            if (currentUserId == order.UserId)
                order.Requests = await requestRepository.GetByOrderAsync(id);

            var model = Mapper.Map<OrderShowModel>(order);
            model.RequestSended = await requestRepository.GetAsync(id, currentUserId) != null;
            return model;
        }

        public async Task<IEnumerable<OrderTopModel>> GetByUserAsync(string userId, OrderStatuses status)
        {
            var orders = await repository.GetByUserAsync(userId, status);
            return Mapper.Map<IEnumerable<OrderTopModel>>(orders);
        }

        public async Task<OrderEditModel> GetToEditAsync(Guid id)
        {
            var order = await repository.GetAsync(id);
            if (order == null)
                return null;
            var model = Mapper.Map<OrderEditModel>(order);
            return model;
        }

        public async Task<IEnumerable<OrderTopModel>> GetTopAsync(OrderTopFilterModel model)
        {
            const int range = 30;
            var offset = (model.Page - 1) * 30;

            var filter = Mapper.Map<OrderTopFilter>(model);
            filter.Range = range;
            filter.Offset = offset;

            var orders = await repository.GetByFilterAsync(filter);
            return Mapper.Map<IEnumerable<OrderTopModel>>(orders);
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id)
        {
            var order = await repository.GetAsync(id);
            if (order == null)
                return DataServiceResult.Failed("Шаблон заказа не найден");

            var isDealsExist = await Database.GetRepo<DealRepository, Deal>().IsExistByOrderAsync(id);
            if (isDealsExist)
                return DataServiceResult.Failed("Нельзя удалить заказ, по которому есть сделки");
            try
            {
                FilesHelper.DeleteFile(order.ImageSrc);
                await repository.RemoveAsync(id);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении заказа", e);
            }
        }

        public async Task<DataServiceResult> RemoveRequestAsync(string executerId, Guid orderId)
        {
            var order = await repository.GetAsync(orderId);
            if (order.ExecuterId == executerId)
                return DataServiceResult.Failed("Вы не можете удалить заявку, поскольку вас выбрали исполнителем");
            var orderRequest = await requestRepository.GetAsync(executerId, orderId);
            if (orderRequest == null)
                return DataServiceResult.Failed("Заявки не существует");
            try
            {
                await requestRepository.RemoveAsync(orderRequest);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении заявки на заказ", e);
            }
        }

        public async Task<DataServiceResult> UpdateAsync(OrderEditModel model)
        {
            try
            {
                var order = await repository.GetAsync(model.Id);
                if (order == null)
                    return DataServiceResult.Failed("Заказ не найден");
                if(order.UserId != model.UserId)
                    return DataServiceResult.Failed("Неизвестный пользователь");

                if(model.ImageStream != null)
                {
                    FilesHelper.DeleteFile(order.ImageSrc);
                    order.ImageSrc = FilesHelper.CreateImage(PathConstants.PublicVirtualImageFolder, model.ImageStream);
                }
                order.Title = model.Header;
                order.Description = model.Description;
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при обновлении заказа", e);
            }
        }
    }
}