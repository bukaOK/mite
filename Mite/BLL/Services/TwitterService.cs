using Mite.BLL.Core;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.ExternalServices.Twitter;
using NLog;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mite.BLL.Services
{
    public interface ITwitterService
    {
        Task<DataServiceResult> AddPostAsync(Post post, string postMessage, IList<string> imagesToSend, string tagsStr);
    }
    public class TwitterService : DataService, ITwitterService
    {
        private readonly HttpClient httpClient;

        public TwitterService(IUnitOfWork database, ILogger logger, HttpClient httpClient) : base(database, logger)
        {
            this.httpClient = httpClient;
        }

        public async Task<DataServiceResult> AddPostAsync(Post post, string postMessage, IList<string> imagesToSend, string tagsStr)
        {
            var externalServiceRepo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var twitInfo = await externalServiceRepo.GetAsync(post.UserId, TwitterSettings.DefaultAuthType);

            if (twitInfo != null && !string.IsNullOrEmpty(twitInfo.AccessToken))
            {
                var twitClient = new TwitterClient(twitInfo.AccessToken, httpClient);
                if (post.ContentType != PostContentTypes.Document)
                {
                    var imagesToAdd = new List<string>();
                    foreach (var imgPath in imagesToSend)
                    {
                        var uploadResult = await twitClient.UploadMediaAsync(imgPath);
                        imagesToAdd.Add(uploadResult.MediaId);
                    }
                    var twitRes = await twitClient.PostAsync("https://api.twitter.com/1.1/statuses/update.json", new
                    {
                        status = postMessage,
                        media_ids = string.Join(",", imagesToAdd)
                    });
                }
                else
                {
                    var twitRes = await twitClient.PostAsync("https://api.twitter.com/1.1/statuses/update.json", new
                    {
                        status = postMessage
                    });
                }
                return Success;
            }
            return DataServiceResult.Failed("Не найдены данные по твиттер аккаунту");
        }
    }
}