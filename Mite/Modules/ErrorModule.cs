using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.Modules
{
    public class ErrorModule : IHttpModule
    {
        private static readonly int[] errorCodes = new[] { 404, 500, 403 };
        private readonly ILogger logger = LogManager.GetLogger("LOGGER");

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += EndRequest_Handler;
            context.Error += Error_Handler;
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
            if (errorCodes.Contains(resp.StatusCode))
            {
                //resp.Clear();
                var content = File.ReadAllText(HostingEnvironment.MapPath($"/Views/ErrorPages/{resp.StatusCode}.html"));
                resp.Write(content);
            }
        }
    }
}