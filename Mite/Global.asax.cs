using AutoMapper;
using Mite.Controllers;
using Mite.ExternalServices.WebMoney.Business.Automapper;
using Mite.Modules;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILogger logger = LogManager.GetLogger("LOGGER");
        private static readonly int[] errorCodes = new[] { 404, 500 };

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureAutoMapper();
            //Mapper.Configuration.AssertConfigurationIsValid();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex is HttpException httpEx)
            {
                logger.Warn(httpEx, "Unhandled http exception");
                Response.StatusCode = httpEx.GetHttpCode();
            }
            else
            {
                logger.Error(ex, "Unhandled error");
                Response.StatusCode = 500;
            }
            Server.ClearError();
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var statusCode = Response.StatusCode;
            if (errorCodes.Contains(statusCode))
            {
                Response.Clear();
                var content = File.ReadAllText(HostingEnvironment.MapPath($"/Views/ErrorPages/{statusCode}.html"));
                Response.Write(content);
            }
        }
        private void ConfigureAutoMapper()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(Assembly.GetExecutingAssembly(), 
                Assembly.GetAssembly(typeof(WebMoneyProfile))));
        }
    }
}
