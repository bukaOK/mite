using AutoMapper;
using Microsoft.ApplicationInsights.Extensibility;
using Mite.Controllers;
using Mite.ExternalServices.WebMoney.Business.Automapper;
using Mite.Infrastructure.Binders;
using Mite.Modules;
using NLog;
using System;
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
            BinderConfig.RegisterBinders();
            ConfigureAutoMapper();
            TelemetryConfiguration.Active.DisableTelemetry = true;
            //Mapper.Configuration.AssertConfigurationIsValid();
        }
        private void ConfigureAutoMapper()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(Assembly.GetExecutingAssembly(), 
                Assembly.GetAssembly(typeof(WebMoneyProfile))));
        }
    }
}
