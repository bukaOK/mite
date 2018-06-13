using AutoMapper;
using Microsoft.ApplicationInsights.Extensibility;
using Mite.ExternalServices.WebMoney.Business.Automapper;
using Mite.Infrastructure.Binders;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
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
