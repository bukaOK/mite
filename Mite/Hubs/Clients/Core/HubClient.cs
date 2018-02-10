using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Hubs.Clients.Core
{
    public abstract class HubClient
    {
        protected static IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
    }
}