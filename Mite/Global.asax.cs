using AutoMapper;
using Mite.ExternalServices.WebMoney.Business.Automapper;
using NLog;
using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILogger logger = LogManager.GetLogger("LOGGER");

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
            var exception = Server.GetLastError();
            logger.Error(exception, "Unhandled Error");
        }
        private void ConfigureAutoMapper()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(Assembly.GetExecutingAssembly(), 
                Assembly.GetAssembly(typeof(WebMoneyProfile))
                )
            );
        }
    }
}
