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
        Task<DataServiceResult> AddAsync(double sum, string operationId, string userId, PaymentType paymentType, PaymentStatus status);
        Task<Payment> GetAsync(Guid id);
        Task<DataServiceResult> ChangeStatusAsync(Guid paymentId, PaymentStatus status);
    }
    public class PaymentService : DataService, IPaymentService
    {
        private readonly AppUserManager userManager;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentService(IUnitOfWork database, AppUserManager userManager, ILogger logger) : base(database, logger)
        {
            paymentsRepository = Database.GetRepo<PaymentsRepository, Payment>();
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddAsync(double sum, string operationId, string userId, PaymentType paymentType, PaymentStatus status)
        {
            var payment = new Payment
            {
                OperationId = operationId,
                UserId = userId,
                Sum = sum,
                Date = DateTime.UtcNow,
                PaymentType = paymentType,
                Status = status
            };
            try
            {
                await paymentsRepository.AddAsync(payment);
                return DataServiceResult.Success(payment);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении платежа", e);
            }
        }

        public async Task<DataServiceResult> ChangeStatusAsync(Guid paymentId, PaymentStatus status)
        {
            var payment = await paymentsRepository.GetAsync(paymentId);
            if (payment == null)
                return DataServiceResult.Failed("Не найден платеж");
            try
            {
                payment.Status = status;
                await paymentsRepository.UpdateAsync(payment);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при изменении статуса платежа", e);
            }
        }

        public Task<Payment> GetAsync(Guid id)
        {
            return paymentsRepository.GetAsync(id);
        }
    }
}