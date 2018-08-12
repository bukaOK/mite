using NLog;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Mite.Modules
{
    public class ErrorModule : IHttpModule
    {
        private static readonly Dictionary<int, string> errorCodes = new Dictionary<int, string>{
            { 404, "NotFound" },
            { 500, "InternalServerError" },
            { 403, "Forbidden" }
        };
        private readonly ILogger logger = LogManager.GetLogger("LOGGER");

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
#if !DEBUG
            context.EndRequest += EndRequest_Handler;
            context.Error += Error_Handler;
#endif
        }

        private void Error_Handler(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var ex = context.Server.GetLastError();
            if (ex is HttpException httpEx)
            {
                logger.Warn(httpEx, "Unhandled http exception");
                context.Response.StatusCode = httpEx.GetHttpCode();
            }
            else
            {
                logger.Error(ex, "Unhandled error");
                context.Response.StatusCode = 500;
            }
            context.Server.ClearError();
        }

        private void EndRequest_Handler(object sender, EventArgs e)
        {
            var resp = HttpContext.Current.Response;

            if (resp.StatusCode == 403 && resp.SubStatusCode == 14)
                resp.StatusCode = 404;

            if (errorCodes.TryGetValue(resp.StatusCode, out string action))
            {
                var routeData = new RouteData();
                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = action;

                var requestContext = new RequestContext(new HttpContextWrapper(HttpContext.Current), routeData);
                var controller = ControllerBuilder.Current.GetControllerFactory().CreateController(requestContext, "Error");
                controller.Execute(requestContext);
            }
        }
    }
}