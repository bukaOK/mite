﻿using Mite.BLL.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using Mite.DAL.Entities;
using Mite.CodeData.Enums;
using Mite.CodeData.Constants;
using NLog;
using Mite.DAL.Repositories;
using Mite.BLL.DTO;

namespace Mite.BLL.Services
{
    public interface ICashService : IDataService
    {
        /// <summary>
        /// Возвращает историю денежных операций пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<OperationModel>> GetPaymentsHistoryAsync(string userId);
        /// <summary>
        /// Получить список рефералов у пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<ReferalModel>> GetReferalsByUserAsync(string userId);
        /// <summary>
        /// Добавляет денежную операцию внутри системы
        /// Если to == null, значит перевод системе(в качестве комиссии например)
        /// </summary>
        /// <param name="from">Id отправителя</param>
        /// <param name="to">Id получателя</param>
        /// <param name="sum">Сумма</param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task AddAsync(string from, string to, double sum, CashOperationTypes type);
        void Add(string from, string to, double sum, CashOperationTypes type);
        Task<double> GetUserCashAsync(string userId);
        /// <summary>
        /// Получить список денежных операций пользователя внутри системы
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<IEnumerable<CashOperation>> GetByTypeAsync(string userId, CashOperationTypes type);
        IEnumerable<CashOperation> GetByType(string userId, CashOperationTypes type);
        Task<bool> IsYandexAuthorized(string userId);
        /// <summary>
        /// Получить авторов, которые разрешили показывать рекламу
        /// </summary>
        /// <returns></returns>
        IEnumerable<UserAdDTO> GetAdUsers();
        DataServiceResult AdSensePay(string to, double sum, DateTime date);
    }
    public class CashService : DataService, ICashService
    {
        private readonly ILogger logger;

        public CashService(IUnitOfWork database, ILogger logger) : base(database)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<OperationModel>> GetPaymentsHistoryAsync(string userId)
        {
            var repo = Database.GetRepo<PaymentsRepository, Payment>();
            var payments = await repo.GetByUserAsync(userId);
            
            return Mapper.Map<IEnumerable<OperationModel>>(payments);
        }
        public Task AddAsync(string from, string to, double sum, CashOperationTypes type)
        {
            var operation = new CashOperation
            {
                FromId = from,
                ToId = to,
                Sum = sum,
                OperationType = type,
                Date = DateTime.UtcNow
            };
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            return repo.AddAsync(operation);
        }
        /// <summary>
        /// Список рефералов пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ReferalModel>> GetReferalsByUserAsync(string userId)
        {
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            //Получаем список операций с рефералами
            var operations = await repo.GetByOperationTypeAsync(userId, CashOperationTypes.Referal, true);

            var referals = new List<ReferalModel>();
            
            foreach(var operation in operations)
            {
                var referal = referals.FirstOrDefault(x => 
                string.Equals(x.UserName, operation.From.UserName, StringComparison.OrdinalIgnoreCase));

                if(referal == default(ReferalModel))
                {
                    referal = new ReferalModel
                    {
                        UserName = operation.From.UserName
                    };
                    referals.Add(referal);
                }
                referal.Income += operation.Sum;
            }
            return referals;
        }

        public async Task<double> GetUserCashAsync(string userId)
        {
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            var paymentsRepo = Database.GetRepo<PaymentsRepository, Payment>();

            var cashOperations = await repo.GetListAsync(userId);
            var payments = await paymentsRepo.GetByUserAsync(userId);

            var cash = 0.0;
            foreach (var payment in payments)
            {
                cash += payment.Sum;
            }
            logger.Info($"PaymentsSum: {cash.ToString()}");
            foreach (var operation in cashOperations)
            {
                if (userId == operation.FromId)
                {
                    cash -= operation.Sum;
                }
                else if (userId == operation.ToId)
                {
                    cash += operation.Sum;
                }
            }
            logger.Info($"AllSum: {cash.ToString()}");
            return Math.Round(cash, 2);
        }

        public async Task<bool> IsYandexAuthorized(string userId)
        {
            var repo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var service = await repo.GetAsync(userId, YaMoneySettings.DefaultAuthType);
            return service != null;
        }

        public Task<IEnumerable<CashOperation>> GetByTypeAsync(string userId, CashOperationTypes type)
        {
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            return repo.GetByOperationTypeAsync(userId, type);
        }

        public IEnumerable<CashOperation> GetByType(string userId, CashOperationTypes type)
        {
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            return repo.GetByOperationType(userId, type);
        }
        public IEnumerable<UserAdDTO> GetAdUsers()
        {
            var repo = Database.GetRepo<UserRepository, User>();
            return repo.GetAdUsers();
        }

        public void Add(string from, string to, double sum, CashOperationTypes type)
        {
            var operation = new CashOperation
            {
                FromId = from,
                ToId = to,
                Sum = sum,
                OperationType = type,
                Date = DateTime.UtcNow
            };
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            repo.Add(operation);
        }

        public DataServiceResult AdSensePay(string to, double sum, DateTime date)
        {
            var repo = Database.GetRepo<CashOperationsRepository, CashOperation>();
            var userOperations = repo.GetByOperationType(to, CashOperationTypes.GoogleAd);
            var existingOperation = userOperations.FirstOrDefault(x => x.Date.DayOfYear == DateTime.Now.DayOfYear);
            if (existingOperation != null)
                return DataServiceResult.Success();
            var operation = new CashOperation
            {
                FromId = null,
                ToId = to,
                Sum = sum,
                OperationType = CashOperationTypes.GoogleAd,
                Date = date
            };
            try
            {
                repo.Add(operation);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при добавлении платежа AdSense в базу: " + e.Message);
                return DataServiceResult.Failed();
            }

        }
    }
}