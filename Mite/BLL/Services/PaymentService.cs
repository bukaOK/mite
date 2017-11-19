using Mite.BLL.Core;
using System;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.CodeData.Enums;
using Mite.DAL.Repositories;
using NLog;

namespace Mite.BLL.Services
{
    public interface IPaymentService : IDataService
    {
        Task<DataServiceResult> AddAsync(double sum, string operationId, string userId, PaymentType paymentType);
        /// <summary>
        /// Платеж выполнен успешно
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
    }
    public class PaymentService : DataService, IPaymentService
    {
        private readonly AppUserManager userManager;

        public PaymentService(IUnitOfWork database, AppUserManager userManager, ILogger logger) : base(database, logger)
        {
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddAsync(double sum, string operationId, string userId, PaymentType paymentType)
        {
            var repo = Database.GetRepo<PaymentsRepository, Payment>();
            var payment = new Payment
            {
                OperationId = operationId,
                UserId = userId,
                Sum = sum,
                Date = DateTime.UtcNow,
                PaymentType = paymentType
            };
            try
            {
                await repo.AddAsync(payment);
                return DataServiceResult.Success(payment);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении платежа", e);
            }
        }
    }
}