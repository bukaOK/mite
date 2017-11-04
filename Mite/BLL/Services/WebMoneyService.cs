using Mite.BLL.Core;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.ExternalServices.WebMoney;
using Mite.DAL.Entities;
using System.Net.Http;
using Mite.ExternalServices.WebMoney.Params;
using Mite.DAL.Repositories;
using System;
using Mite.CodeData.Enums;
using System.Threading.Tasks;
using Mite.ExternalServices.WebMoney.Business;

namespace Mite.BLL.Services
{
    public interface IWebMoneyService : IDataService
    {
        Task<DataServiceResult> PayInAsync(string clientPhone, double sum);
        Task<DataServiceResult> ConfirmPayInAsync(int invoiceId, string confirmationCode);
    }
    public class WebMoneyService : DataService, IWebMoneyService
    {
        private readonly WMExpressPayment payment;

        public WebMoneyService(IUnitOfWork database, ILogger logger, HttpClient client) : base(database, logger)
        {
            payment = new WMExpressPayment(logger, client);
        }
        public async Task<DataServiceResult> PayInAsync(string clientPhone, double sum)
        {
            var paymentsRepo = Database.GetRepo<PaymentsRepository, Payment>();
            var random = new Random();
            int orderId;
            Payment existingPayment;
            //Находим уникальный orderId
            do
            {
                orderId = random.Next(0, int.MaxValue);
                existingPayment = await paymentsRepo.GetByOperationAsync(orderId.ToString(), PaymentType.WebMoney);
            } while (existingPayment != null);
            var result = await payment.PayInAsync(new ExpressPaymentParams
            {
                OrderId = orderId,
                Amount = sum,
                ClientPhone = new Phone(clientPhone),
            });
            if (result.Succeeded)
            {
                return DataServiceResult.Success(result.ResultData);
            }
            return DataServiceResult.Failed(result.ErrorMessage);
        }
        public async Task<DataServiceResult> ConfirmPayInAsync(int invoiceId, string confirmationCode)
        {
            var result = await payment.ConfirmPayInAsync(new ExpressPaymentConfirmParams
            {
                ConfirmCode = confirmationCode,
                InvoiceId = invoiceId
            });
            if (result.Succeeded)
            {
                return DataServiceResult.Success(result.ResultData);
            }
            return DataServiceResult.Failed(result.ErrorMessage);
        }
    }
}