using Facebook;
using Mite.BLL.Core;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mite.BLL.Services
{
    public interface IFacebookService
    {
        Task<DataServiceResult> AddPostAsync(Post post, string fbPageId, string postMessage,
            IList<string> imagesToSend, string domain, string tagsStr);
    }
    public class FacebookService : DataService, IFacebookService
    {
        public FacebookService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
        }

        public async Task<DataServiceResult> AddPostAsync(Post post, string fbPageId, string postMessage, IList<string> imagesToSend, string domain, string tagsStr)
        {
            var externalServiceRepo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var fbInfo = await externalServiceRepo.GetAsync(post.UserId, FacebookSettings.DefaultAuthType);

            if (fbInfo != null && !string.IsNullOrEmpty(fbInfo.AccessToken) && !string.IsNullOrEmpty(fbPageId))
            {
                var fbClient = new FacebookClient(fbInfo.AccessToken)
                {
                    AppId = FacebookSettings.AppId,
                    AppSecret = FacebookSettings.AppSecret
                };
                if (post.ContentType != PostContentTypes.Document)
                {
                    var imagesToAdd = new List<string>();
                    foreach (var img in imagesToSend)
                    {
                        var fbImgUploadResp = await fbClient.PostTaskAsync($"https://graph.facebook.com/{fbPageId}/photos", new
                        {
                            url = $"{domain}/images/post/{post.Id}?watermark={post.WatermarkId != null}&resize=false"
                        }) as IDictionary<string, object>;
                        imagesToAdd.Add(fbImgUploadResp["id"].ToString());
                    }
                    var fbPostParams = new Dictionary<string, string>
                    {
                        {"message", post.Description + "\n" + tagsStr }
                    };
                    for (var i = 0; i < imagesToAdd.Count; i++)
                    {
                        fbPostParams.Add("attached_media", $"{{\"media_fbid\":\"{imagesToAdd[i]}\"}}");
                    }
                    var fbPostResp = await fbClient.PostTaskAsync($"https://graph.facebook.com/{fbPageId}/feed", fbPostParams);
                }
                else
                {
                    var fbPostResp = await fbClient.PostTaskAsync($"https://graph.facebook.com/{fbPageId}/feed", new
                    {
                        message = postMessage
                    });
                }
                return Success;
            }
            return DataServiceResult.Failed("Не найдены данные по facebook аккаунту");
        }
    }
}