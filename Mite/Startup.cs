using Autofac;
using Duke.Owin.VkontakteMiddleware;
using Duke.Owin.VkontakteMiddleware.Provider;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Twitter;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Constants;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Migrations;
using Owin;
using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(Mite.Startup))]
namespace Mite
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Database.SetInitializer(new DbInitializer(app.GetDataProtectionProvider()));
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());
            var dbContext = new AppDbContext();
            dbContext.Database.Initialize(false);
            var apiConfig = ConfigureWebApi();
            var container = AutofacDI.Initialize(app, apiConfig);
            ConfigureAuth(app, container);
            app.UseWebApi(apiConfig);
            app.MapSignalR();

            CitiesInitializer.Initialize();
            HangfireConfig.Initialize(app, container);
        }
        private void ConfigureAuth(IAppBuilder app, IContainer diContainer)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<AppUserManager, User>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie))
                },
                CookieName = "MiteCookie",
                LogoutPath = new PathString("/account/logoff")
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");
            app.UseTwitterAuthentication(new TwitterAuthenticationOptions
            {
                ConsumerKey = "wqHlrgtp5UYi7mZtiBWpVawbN",
                ConsumerSecret = "hhvpYmzxFLHTiH3aOIMukkIepZ3hRsMxeQnKcE2uZmrZDu8y1s",
                AuthenticationType = TwitterSettings.DefaultAuthType,
                Provider = new TwitterAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        return Task.Run(() =>
                        {
                            context.Identity.AddClaim(new Claim(ClaimConstants.ExternalServiceToken, context.AccessToken));
                        });
                    }
                }
            });
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AppId = "1709833739044048",
                AppSecret = "f8e4260827532461cf2eb39584f79fd7",
                AuthenticationType = FacebookSettings.DefaultAuthType,
                Provider = new FacebookAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        return Task.Run(() =>
                        {
                            context.Identity.AddClaim(new Claim(ClaimConstants.ExternalServiceToken, context.AccessToken));
                        });
                    }
                }
            });

            app.UseVkontakteAuthentication(new VkAuthenticationOptions
            {
                AppId = "6013159",
                AppSecret = "bfVzsqk5oyHvlGUwLM8P",
                AuthenticationType = VkSettings.DefaultAuthType,
                Caption = "Вконтакте",
                Provider = new VkAuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        return Task.Run(() =>
                        {
                            context.Identity.AddClaim(new Claim(ClaimConstants.ExternalServiceToken, context.AccessToken));
                        });
                    }
                }
            });
            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute("DefaultNameApi", "api/{controller}/{name}", new { name = RouteParameter.Optional });
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            return config;
        }
    }
}
