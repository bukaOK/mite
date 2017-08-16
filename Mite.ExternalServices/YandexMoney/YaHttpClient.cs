using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Yandex.Money.Api.Sdk.Exceptions;
using Yandex.Money.Api.Sdk.Interfaces;

namespace Mite.ExternalServices.YandexMoney
{
    public class YaHttpClient : IHttpClient
    {
        private readonly Uri BaseUri = new Uri("https://money.yandex.ru");
        private readonly HttpClient httpClient;
        private readonly Authenticator authenticator;

        /// <summary>
        /// От кого отправляется запрос
        /// </summary>
        public string FromUserId { get; set; }

        public YaHttpClient(HttpClient httpClient, Authenticator authenticator)
        {
            this.httpClient = httpClient;
            this.authenticator = authenticator;
        }
        public async Task<Stream> UploadDataAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var yaParams = new Dictionary<string, string>();
            var token = AuthenticationToken;

            request.AppendItemsTo(yaParams);
            var content = new FormUrlEncodedContent(yaParams);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var reqUri = new Uri(BaseUri, request.RelativeUri);
            var response = await httpClient.PostAsync(reqUri, content, cancellationToken);

            var respStr = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadAsStreamAsync();

            if (response.Headers == null || response.Headers.WwwAuthenticate == null)
                return null;

            var responseError = string.Empty;

            var authenticationHeaderValue = response
                .Headers
                .WwwAuthenticate
                .FirstOrDefault(x => x.Scheme == "Bearer");

            if (authenticationHeaderValue != null)
                responseError = authenticationHeaderValue.Parameter;

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new InvalidRequestException(responseError);

                case HttpStatusCode.Unauthorized:
                    throw new InvalidTokenException(responseError);

                case HttpStatusCode.Forbidden:
                    throw new InsufficientScopeException(responseError);

                default:
                    throw new IOException(responseError);
            }
        }
        private string AuthenticationToken
        {
            get
            {
                if (authenticator.Token != null)
                    return authenticator.Token;
                throw new NullReferenceException("Не задан токен аутентификации");
            }
        }
    }
}