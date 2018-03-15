﻿using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Mite.Modules.Vk.Provider;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Mite.Modules.Vk
{
    public class VkAuthenticationMiddleware : AuthenticationMiddleware<VkAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a <see cref="VkAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        public VkAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, VkAuthenticationOptions options)
            : base(next, options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (options == null)
                throw new ArgumentException("options can't be null or empty");

            _logger = app.CreateLogger<VkAuthenticationMiddleware>();


            if (Options.Provider == null)
            {
                Options.Provider = new VkAuthenticationProvider();
            }
            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(typeof(VkAuthenticationMiddleware).FullName, Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
            if (String.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                Options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            _httpClient = new HttpClient(ResolveHttpMessageHandler(Options))
            {
                Timeout = Options.BackchannelTimeout,
                MaxResponseContentBufferSize = 1024 * 1024 * 10 // 10 MB
            };
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests. 
        /// Called at start of every page request.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="VkkAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<VkAuthenticationOptions> CreateHandler()
        {
            return new VkAuthenticationHandler(_httpClient, _logger);
        }

        private static HttpMessageHandler ResolveHttpMessageHandler(VkAuthenticationOptions options)
        {
            var handler = options.BackchannelHttpHandler ?? new WebRequestHandler();

            // If they provided a validator, apply it or fail.
            if (options.BackchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = handler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException("An ICertificateValidator cannot be specified at the same time as an HttpMessageHandler unless it is a WebRequestHandler.");
                }
                webRequestHandler.ServerCertificateValidationCallback = options.BackchannelCertificateValidator.Validate;
            }

            return handler;
        }
    }
}