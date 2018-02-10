using Mite.ExternalServices.VkApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Messages
{
    /// <summary>
    /// Отправка сообщения сообщества
    /// </summary>
    public class SendRequest : VkRequest<string>
    {
        public SendRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        [VkParam("user_id")]
        public string UserId { get; set; }
        [VkParam("random_id")]
        public int? RandomId { get; set; }
        [VkParam("message")]
        public string Message { get; set; }
        [VkParam("attachment")]
        public string Attachment { get; set; }
        public override string Method => "messages.send";
    }
}
