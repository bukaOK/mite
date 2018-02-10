using Mite.Hubs.Clients.Core;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Hubs.Clients
{
    public class NotifyHubClient : HubClient
    {
        public static void NewMessage(ChatMessageModel message)
        {
            foreach (var re in message.Recipients)
            {
                if (re.Id != message.Sender.Id)
                {
                    HubContext.Clients.Group(re.Id).addMessage(JsonConvert.SerializeObject(message));
                }
            }
        }
    }
}