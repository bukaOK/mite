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
    /// Разрешена ли отправка сообщений сообщества
    /// </summary>
    public class SendingAllowedRequest : VkRequest<SendingAllowedResponse>
    {
        public SendingAllowedRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }

        [VkParam("group_id")]
        public string GroupId { get; set; }
        [VkParam("user_id")]
        public string UserId { get; set; }

        public override string Method => "messages.isMessagesFromGroupAllowed";
    }
}
