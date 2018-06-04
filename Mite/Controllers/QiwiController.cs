using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class QiwiController : BaseController
    {
        private readonly IPaymentService paymentService;

        public QiwiController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        public async Task<ActionResult> Notify(string bill_id, string status)
        {
            var auth = Request.Headers["Authorization"];
            var idPass = Encoding.UTF8.GetString(Convert.FromBase64String(auth.Split(' ')[1])).Split(':');
            if (idPass[0] != QiwiSettings.ApiId || idPass[1] != QiwiSettings.ApiPassword)
                return Forbidden();

            if(Guid.TryParse(bill_id, out Guid paymentId))
            {
                var payment = await paymentService.GetAsync(paymentId);
                if(payment.Status == PaymentStatus.Waiting && status == "paid")
                {
                    var result = await paymentService.ChangeStatusAsync(payment.Id, PaymentStatus.Payed);
                    if (result.Succeeded)
                        return Content("<?xml version=\"1.0\"?><result><result_code>0</result_code></result>", "text/xml");
                    return InternalServerError();
                }
            }
            return BadRequest();
        }
    }
}