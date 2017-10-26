using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Owin;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Autofac;
using Autofac.Integration.WebApi;
using Autofac.Integration.Mvc;
using System.Net.Http;
using Mite.ExternalServices.YandexMoney;
using Yandex.Money.Api.Sdk.Interfaces;
using NLog;
using Mite.BLL.Core;
using Mite.BLL.Infrastructure;
using Mite.Infrastructure;
using System.Web;
using Mite.Modules;
using Autofac.Integration.SignalR;

namespace Mite
{
    public static class AutofacDI
    {
        public static IContainer Initialize(IAppBuilder app, HttpConfiguration apiConfiguration)
        {
            var builder = new ContainerBuilder();
            var executingAssembly = Assembly.GetExecutingAssembly();

            builder.RegisterControllers(executingAssembly);
            builder.RegisterApiControllers(executingAssembly);
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            RegisterComponents(builder, app);

            var container = builder.Build();
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            apiConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            //app.UseAutofacMiddleware(container);

            app.UseAutofacWebApi(apiConfiguration);
            app.UseAutofacMvc();

            return container;
        }

        private static void RegisterComponents(ContainerBuilder builder, IAppBuilder app)
        {
            var dataProtectionProvider = app.GetDataProtectionProvider();

            builder.RegisterType<AppDbContext>().InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

            builder.Register(c => HttpContext.Current.GetOwinContext().Authentication).As<IAuthenticationManager>().InstancePerLifetimeScope();
            builder.Register(c => new AppUserManager(new UserStore<User>(c.Resolve<AppDbContext>()),
                dataProtectionProvider)).InstancePerLifetimeScope();
            builder.RegisterType<AppSignInManager>();
            builder.RegisterType<AppRoleManager>();

            builder.RegisterType<ServiceBuilder>().As<IServiceBuilder>();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IDataService>()
                .AsImplementedInterfaces();

            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<YaHttpClient>().As<IHttpClient>();
            builder.RegisterType<Authenticator>().InstancePerLifetimeScope();

            builder.Register(c => LogManager.GetLogger("LOGGER")).As<ILogger>().SingleInstance();

            builder.RegisterType<GeoMiddleware>().InstancePerLifetimeScope();
        }
    }
}