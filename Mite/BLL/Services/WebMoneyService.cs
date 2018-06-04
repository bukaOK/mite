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
using Mite.ExternalServices.WebMoney.Responses;

namespace Mite.BLL.Services
{
    public interface IWebMoneyService : IDataService
    {
        Task<DataServiceResult> PayInAsync(string clientPhone, double sum);
        Task<DataServiceResult> ConfirmPayInAsync(int invoiceId, string confirmationCode, string userId);
    }
    public class WebMoneyService : DataService, IWebMoneyService
    {
        private readonly WMExpressPayment payment;
        private readonly IPaymentService paymentService;

        public WebMoneyService(IUnitOfWork database, ILogger logger, HttpClient client, IPaymentService paymentService) : base(database, logger)
        {
            payment = new WMExpressPayment(logger, client);
            this.paymentService = paymentService;
        }
        public async Task<DataServiceResult> PayInAsync(string clientPhone, double sum)
        {
            var paymentsRepo = Database.GetRepo<PaymentsRepository, Payment>();
            //Id нового запроса всегда должен быть больше предыдущего, находим последнюю операцию
            var lastOperation = await paymentsRepo.GetLastOperationAsync(PaymentType.WebMoney);
            var orderId = lastOperation == null 
                ? 0 : int.Parse(lastOperation.OperationId);
            orderId++;
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
        public async Task<DataServiceResult> ConfirmPayInAsync(int invoiceId, string confirmationCode, string userId)
        {
            var result = await payment.ConfirmPayInAsync(new ExpressPaymentConfirmParams
            {
                ConfirmCode = confirmationCode,
                InvoiceId = invoiceId
            });
            if (result.Succeeded)
            {
                var response = result.ResultData as ExpressPaymentConfirmResponse;
                var payResult = await 
                    paymentService.AddAsync(response.Operation.Amount, response.Operation.TransactionId.ToString(), userId, PaymentType.WebMoney,
                        PaymentStatus.Payed);
                if(payResult.Succeeded)
                    return DataServiceResult.Success(response.UserDescription);
            }
            return DataServiceResult.Failed(result.ErrorMessage);
        }
    }
}