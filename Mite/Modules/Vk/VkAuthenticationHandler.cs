using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Mite.ExternalServices.VkApi.Auth;
using Mite.ExternalServices.VkApi.Users;
using Mite.Modules.Vk.Provider;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Mite.Modules.Vk
{
    public class VkAuthenticationHandler : AuthenticationHandler<VkAuthenticationOptions>
    {
        private const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
        private const string TokenEndpoint = "https://oauth.vk.com/access_token";
        private const string GraphApiEndpoint = "https://api.vk.com/method/";

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public VkAuthenticationHandler(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        //<summary>step 1
        //called at the end of server request after site controllers
        //if client not autorized 401 - redirect to vk.com - It is start point of the authorization process
        //Redirect user to vk.com where he need loging and allow access to your app
        //after that redirect back to {host}/signin-vkontakte
        //</summary
        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            //Helper checking if that module called for login
            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                var baseUri = Request.Scheme + Uri.SchemeDelimiter + Request.Host + Request.PathBase;
                var currentUri = baseUri + Request.Path + Request.QueryString;
                var redirectUri = baseUri + Options.CallbackPath;

                var properties = challenge.Properties;
                if (string.IsNullOrEmpty(properties.RedirectUri))
                {
                    properties.RedirectUri = currentUri;
                }

                // OAuth2 10.12 CSRF
                GenerateCorrelationId(properties);

                var state = Options.StateDataFormat.Protect(properties);

                Options.StoreState = state;

                var authorizationEndpoint = $"https://oauth.vk.com/authorize?client_id={Options.AppId}&redirect_uri={redirectUri}" +
                    $"&scope={string.Join(",", Options.Scope)}&v={Options.Version}";

                Response.Redirect(authorizationEndpoint);
            }
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Шаг 2.0
        /// Вызывается при старте запроса - проверяет совпадает ли запрос с "{host}/signin-vkontakte" url {?code=*******************}.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> InvokeAsync()
        {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                var ticket = await AuthenticateAsync(); //call Task<AuthenticationTicket> AuthenticateCoreAsync() step 2.3
                if (ticket == null)
                {
                    _logger.WriteWarning("Invalid return state, unable to redirect.");
                    Response.StatusCode = 500;
                    return true;
                }

                var context = new VkReturnEndpointContext(Context, ticket)
                {
                    SignInAsAuthenticationType = Options.SignInAsAuthenticationType,
                    RedirectUri = ticket.Properties.RedirectUri
                };

                await Options.Provider.ReturnEndpoint(context);


                if (context.SignInAsAuthenticationType != null &&
                    context.Identity != null)
                {
                    var grantIdentity = context.Identity;
                    if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                    {
                        grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType, grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
                    }
                    Context.Authentication.SignIn(context.Properties, grantIdentity);
                }

                if (!context.IsRequestCompleted && context.RedirectUri != null)
                {
                    string redirectUri = context.RedirectUri;
                    if (context.Identity == null)
                    {
                        // add a redirect hint that sign-in failed in some way
                        redirectUri = WebUtilities.AddQueryString(redirectUri, "error", "access_denied");
                    }
                    Response.Redirect(redirectUri);
                    context.RequestCompleted();
                }

                return context.IsRequestCompleted;
            }

            return false;
        }

        /// <summary>
        /// Шаг 2.1
        /// Создает тикет после того, как клиент возвращается с вк, здесь фактически происходит авторизация
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            AuthenticationProperties properties = null;

            try
            {
                var code = "";

                var query = Request.Query;
                var values = query.GetValues("code");

                if (values != null && values.Count == 1)
                {
                    code = values[0];
                }

                properties = Options.StateDataFormat.Unprotect(Options.StoreState);
                if (properties == null)
                {
                    return null;
                }

                // OAuth2 10.12 CSRF
                if (!ValidateCorrelationId(properties, _logger))
                {
                    return new AuthenticationTicket(null, properties);
                }

                var requestPrefix = Request.Scheme + Uri.SchemeDelimiter + Request.Host;
                var redirectUri = requestPrefix + Request.PathBase + Options.CallbackPath;

                var tokenRequest = $"{TokenEndpoint}?client_id={Options.AppId}&client_secret={Options.AppSecret}" +
                    $"&code={code}&redirect_uri={redirectUri}";

                var tokenResponse = await _httpClient.GetAsync(tokenRequest, Request.CallCancelled);
                tokenResponse.EnsureSuccessStatusCode();
                var text = await tokenResponse.Content.ReadAsStringAsync();
                //IFormCollection form = WebHelpers.ParseForm(text);
                var jsonResponse = JsonConvert.DeserializeObject<TokenResponse>(text);

                var usersReq = new UsersGetRequest(_httpClient, jsonResponse.AccessToken)
                {
                    UserIds = jsonResponse.UserId,
                    Fields = "nickname,domain,photo_200"
                };
                var usersResp = await usersReq.PerformAsync();
                var context = new VkAuthenticatedContext(Context, usersResp.ToArray()[0], jsonResponse.AccessToken, jsonResponse.ExpiresSeconds)
                {
                    Identity = new ClaimsIdentity(Options.AuthenticationType, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType)
                };

                if (!string.IsNullOrEmpty(context.Id))
                {
                    context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id, XmlSchemaString, Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.DefaultName))
                {
                    context.Identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, context.DefaultName, XmlSchemaString, Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.FullName))
                {
                    context.Identity.AddClaim(new Claim("urn:vkontakte:name", context.FullName, XmlSchemaString, Options.AuthenticationType));
                }
                if (!string.IsNullOrEmpty(context.PhotoLink))
                {
                    context.Identity.AddClaim(new Claim("urn:vkontakte:link", context.PhotoLink, XmlSchemaString, Options.AuthenticationType));
                }
                context.Properties = properties;

                await Options.Provider.Authenticated(context);

                return new AuthenticationTicket(context.Identity, context.Properties);

            }
            catch (Exception ex)
            {
                _logger.WriteError(ex.Message);
            }
            return new AuthenticationTicket(null, properties);
        }
    }
}