using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.Constants;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly Authenticator authenticator;

        /// <summary>
        /// От кого отправляется запрос
        /// </summary>
        public string FromUserId { get; set; }

        public YaHttpClient(HttpClient httpClient, IUnitOfWork unitOfWork, Authenticator authenticator)
        {
            this.httpClient = httpClient;
            this.unitOfWork = unitOfWork;
            this.authenticator = authenticator;
        }
        public async Task<Stream> UploadDataAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var yaParams = new Dictionary<string, string>();
            var token = await GetTokenAsync();

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
                    await RemoveTokenFromDatabase();
                    throw new InvalidTokenException(responseError);

                case HttpStatusCode.Forbidden:
                    throw new InsufficientScopeException(responseError);

                default:
                    throw new IOException(responseError);
            }
        }
        private async Task<string> GetTokenAsync()
        {
            if (authenticator.Token != null)
                return authenticator.Token;
            var userId = HttpContext.Current.User.Identity.GetUserId();

            var repo = unitOfWork.GetRepo<ExternalServiceRepository, ExternalService>();
            var service = await repo.GetAsync(userId, YaMoneySettings.DefaultAuthType);
            if (service == null)
                return string.Empty;
            return service.AccessToken;
        }
        /// <summary>
        /// Удаляет токен из базы данных
        /// </summary>
        /// <returns></returns>
        private async Task RemoveTokenFromDatabase()
        {
            var repo = unitOfWork.GetRepo<ExternalServiceRepository, ExternalService>();

            var userId = FromUserId;
            var service = await repo.GetAsync(userId, YaMoneySettings.DefaultAuthType);
            if (service != null)
            {
                await repo.RemoveAsync(service.Id);
            }
        }
    }
}