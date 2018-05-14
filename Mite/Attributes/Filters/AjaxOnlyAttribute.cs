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
        public Type ControllerType { get; }

        public AjaxOnlyAttribute(string indexPage)
        {
            IndexPage = indexPage;
        }
        public AjaxOnlyAttribute(string indexPage, Type controllerType)
        {
            IndexPage = indexPage;
            ControllerType = controllerType;
        }
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated && !filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.RequestContext.RouteData.Values["action"].ToString() != IndexPage
                && !filterContext.IsChildAction)
            {
                var view = new ViewResult
                {
                    ViewName = IndexPage
                };
                filterContext.Result = view;
            }
        }
    }
}