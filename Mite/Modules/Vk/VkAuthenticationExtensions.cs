using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Modules.Vk
{
    public static class VkAuthenticationExtensions
    {
        public static IAppBuilder UseVkontakteAuthentication(this IAppBuilder app, VkAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(VkAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Vkontakte
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="appId">The appId assigned by Vkontakte</param>
        /// <param name="appSecret">The appSecret assigned by Vkontakte</param>
        /// <param name="scope">The permissions list. Comma separated. Like "audio,video,photos"</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseVkontakteAuthentication(
            this IAppBuilder app,
            string appId,
            string appSecret,
            string[] scope)
        {
            return UseVkontakteAuthentication(
                app,
                new VkAuthenticationOptions
                {
                    AppId = appId,
                    AppSecret = appSecret,
                    Scope = scope
                });
        }
    }
}