using Microsoft.Owin;
using Microsoft.Owin.Security;
using Mite.CodeData.Constants;
using Mite.Modules.Vk.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Mite.Modules.Vk
{
    public class VkAuthenticationOptions : AuthenticationOptions
    {
        public VkAuthenticationOptions() : base(VkSettings.DefaultAuthType)
        {
            Caption = VkSettings.DefaultAuthType;
            CallbackPath = new PathString("/signin-vkontakte");
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = null;
            Version = "5.73";
            BackchannelTimeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// Gets or sets the appId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the app secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Vkontakte.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Vkontakte.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with Vkontakte.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned.
        /// The middleware will process this request when it arrives.
        /// Default value is "/signin-vkontakte".
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IVkAuthenticationProvider"/> used to handle authentication events.
        /// </summary>
        public IVkAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the site redirect url after login 
        /// </summary>
        public string StoreState { get; set; }

        /// <summary>
        /// A list of permissions to request.
        /// Can be something like that "audio,video,pages" and etc. More info http://vk.com/dev/permissions
        /// </summary>
        public string[] Scope { get; set; }

        /// <summary>
        /// Get or set vk.com api version.
        /// </summary>
        public string Version { get; set; }
    }
}