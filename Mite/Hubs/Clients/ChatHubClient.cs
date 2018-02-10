using Microsoft.AspNet.SignalR;
using Mite.Hubs;
using Mite.Hubs.Clients.Core;
using Mite.Models;
using System.Collections.Generic;

namespace Mite.Hubs.Clients
{
    public class ChatHubClient : HubClient
    {
        public static void AddMessage(ChatMessageModel message)
        {
            if (message.Attachments == null)
                message.Attachments = new List<MessageAttachmentModel>();
            foreach (var re in message.Recipients)
            {
                if (re.Id != message.Sender.Id)
                {
                    HubContext.Clients.Group(re.Id).addMessage(message);
                }
            }
        }
        public static void AddPublicMessage(ChatMessageModel message)
        {
            if (message.Attachments == null)
                message.Attachments = new List<MessageAttachmentModel>();
            HubContext.Clients.All.addPublicMessage(message);
        }
    }
}