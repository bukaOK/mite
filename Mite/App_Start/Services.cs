using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Net;
using System.Net.Http;

namespace Mite
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var from = "dispatch@mitegroup.ru";
            var to = message.Destination;
            var client = new SmtpClient("smtp.yandex.ru", 25)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from, "Evd$utTC"),
                EnableSsl = true
            };
            
            var mail = new MailMessage(new MailAddress(from, "MiteGroup"), new MailAddress(to))
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };
            return client.SendMailAsync(mail);
        }
    }
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }
}
