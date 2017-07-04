using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Mite.Attributes.Filters
{
    public class AjaxOnlyAttribute : FilterAttribute, IAuthorizationFilter
    {
        private string IndexPage { get; }

        public AjaxOnlyAttribute(string indexPage)
        {
            IndexPage = indexPage;
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.RequestContext.RouteData.Values["action"].ToString() != IndexPage
                && !filterContext.IsChildAction)
            {
                var view = new ViewResult();
                view.ViewName = IndexPage;
                filterContext.Result = view;
            }
        }
    }
}