using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit;

namespace Mite
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            const string from = "dispatch@mitegroup.ru";
            var msg = new MimeMessage();

            msg.From.Add(new MailboxAddress("MiteGroup", from));
            msg.To.Add(new MailboxAddress(message.Destination));
            msg.Subject = message.Subject;

            msg.Body = new TextPart(TextFormat.Html)
            {
                Text = message.Body
            };
            using(var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 465, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(from, "Evd$utTC");
                await client.SendAsync(msg);

                await client.DisconnectAsync(true);
            }
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
