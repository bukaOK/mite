using Autofac;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.Core;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Infrastructure;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.BLL.ExternalServices.YandexMoney;
using NLog;
using Owin;
using System.Net.Http;
using System.Reflection;
using Yandex.Money.Api.Sdk.Interfaces;

namespace Mite.BLL.ConfigServices
{
    public static class AutofacConfigService
    {
        public static void RegisterComponents(ContainerBuilder builder, IAppBuilder app)
        {
            var dataProtectionProvider = app.GetDataProtectionProvider();
            builder.RegisterType<AppDbContext>().InstancePerRequest();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.Register(c => c.Resolve<IOwinContext>().Authentication).As<IAuthenticationManager>().InstancePerRequest();
            builder.Register(c => new AppUserManager(new UserStore<User>(c.Resolve<AppDbContext>()),
                dataProtectionProvider)).InstancePerRequest();
            builder.RegisterType<AppSignInManager>();
            builder.RegisterType<AppRoleManager>();

            builder.RegisterType<ServiceBuilder>().As<IServiceBuilder>();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IDataService>()
                .AsImplementedInterfaces();

            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<YaHttpClient>().As<IHttpClient>();
            builder.RegisterType<Authenticator>().InstancePerRequest();

            builder.Register(c => LogManager.GetLogger("LOGGER")).As<ILogger>().SingleInstance();
        }
    }
}
