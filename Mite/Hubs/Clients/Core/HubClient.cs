using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Hubs.Clients.Core
{
    public abstract class HubClient<THub> where THub : IHub
    {
        private IHubContext hubContext;
        protected IHubContext HubContext
        {
            get
            {
                if (hubContext == null)
                    hubContext = GlobalHost.ConnectionManager.GetHubContext<THub>();
                return hubContext;
            }
        }

        private HttpContext context;
        protected HttpContext Context
        {
            get
            {
                if (context == null)
                    context = HttpContext.Current;
                return context;
            }
        }
    }
}