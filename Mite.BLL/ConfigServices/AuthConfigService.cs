using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Entities;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.ConfigServices
{
    public static class AuthConfigService
    {
        public static void Configure(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/account/login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<AppUserManager, User>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => manager.CreateIdentityAsync(user))
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
    }
}
