using Mite.BLL.Core;
using Mite.CodeData.Constants;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.ExternalServices.DeviantArt;
using Mite.ExternalServices.DeviantArt.Responses;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.BLL.Services
{
    public interface IDeviantArtService
    {
        /// <summary>
        /// Опубликовать работу на DeviantArt
        /// </summary>
        /// <param name="post"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddPostAsync(Post post, string domain);
    }
    public class DeviantArtService : DataService, IDeviantArtService
    {
        private readonly HttpClient httpClient;

        public DeviantArtService(IUnitOfWork database, ILogger logger, HttpClient httpClient) : base(database, logger)
        {
            this.httpClient = httpClient;
        }

        public async Task<DataServiceResult> AddPostAsync(Post post, string domain)
        {
            var externalServiceRepo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var devService = await externalServiceRepo.GetAsync(post.UserId, DeviantArtSettings.DefaultAuthType);
            if (devService == null)
                return DataServiceResult.Failed("Токен DeviantArt не найден");

            var devArtClient = new DeviantArtClient(httpClient);
            try
            {
                var tokenResp = await devArtClient.GetAsync<AccessTokenResponse>(DeviantArtSettings.ApiUrls.RefreshAccessToken, new
                {
                    grant_type = "refresh_token",
                    client_id = DeviantArtSettings.ClientId,
                    client_secret = DeviantArtSettings.ClientSecret,
                    refresh_token = devService.AccessToken
                });

                var submitParams = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("title", post.Title),
                    new KeyValuePair<string, string>("artist_comments", post.Description),
                    new KeyValuePair<string, string>("original_url", post.Content)
                };
                var matureContent = false;
                if (post.Tags != null && post.Tags.Count > 0)
                    foreach (var tag in post.Tags)
                    {
                        if (tag.Name == "18+") matureContent = true;
                        submitParams.Add(new KeyValuePair<string, string>("tags[]", tag.Name));
                    }
                submitParams.Add(new KeyValuePair<string, string>("mature_content", matureContent.ToString().ToLower()));
                var submitResp = await devArtClient.PostAsync<StashSubmitResponse>(DeviantArtSettings.ApiUrls.StashSubmit, submitParams);

                return Success;
            }
            catch(DeviantArtException e)
            {
                logger.Warn($"DeviantArt exception: {e.Message}");
                await externalServiceRepo.RemoveAsync(devService.UserId, DeviantArtSettings.DefaultAuthType);
                return DataServiceResult.Failed();
            }
            catch (Exception e)
            {
                return CommonError("DeviantArt exception", e);
            }
        }
    }
}