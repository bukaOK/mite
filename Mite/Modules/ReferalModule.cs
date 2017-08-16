using Mite.CodeData.Constants;
using Mite.Extensions;
using Mite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Mite.Modules
{
    public class ReferalModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            var sessionModule = (SessionStateModule)context.Modules["Session"];
            sessionModule.Start += SetRefId;
        }
        private void SetRefId(object source, EventArgs e)
        {
            var context = HttpContext.Current;
            if(context.Request.QueryString["refid"] != null && !context.User.Identity.IsAuthenticated)
            {
                context.Session["refid"] = context.Request.QueryString["refid"];
            }
        }
    }
}