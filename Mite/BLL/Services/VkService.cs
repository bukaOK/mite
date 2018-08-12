using Mite.BLL.Core;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.ExternalServices.VkApi.Core;
using Mite.ExternalServices.VkApi.Groups;
using Mite.ExternalServices.VkApi.Photos;
using Mite.ExternalServices.VkApi.Wall;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Mite.BLL.Services
{
    public interface IVkService : IDataService
    {
        Task<DataServiceResult> AddPostAsync(Post post, string vkGroupId, string postMessage,
            IList<string> imagesToSend, string tagsStr);
        Task<DataServiceResult> GetGroupIdAsync(string userId, string domain);
    }
    public class VkService : DataService, IVkService
    {
        private readonly PostsRepository postsRepository;
        private readonly HttpClient httpClient;
        private readonly IExternalServices externalServices;

        public VkService(IUnitOfWork database, ILogger logger, HttpClient httpClient) : base(database, logger)
        {
            postsRepository = database.GetRepo<PostsRepository, Post>();
            this.httpClient = httpClient;
        }

        public async Task<DataServiceResult> AddPostAsync(Post post, string vkGroupId, string postMessage,
            IList<string> imagesToSend, string tagsStr)
        {
            var externalServiceRepo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            var vkInfo = await externalServices.GetAsync(post.UserId, VkSettings.DefaultAuthType);

            if (vkInfo != null && !string.IsNullOrEmpty(vkInfo.AccessToken) && !string.IsNullOrEmpty(vkGroupId))
            {
                if (post.ContentType != PostContentTypes.Document)
                {
                    var vkServerResult = await new PhotoGetWallUploadServerRequest(httpClient, vkInfo.AccessToken).PerformAsync();
                    var url = vkServerResult.UploadUrl;

                    //Список загруженных id для изображений
                    var imagesToAdd = new List<string>();
                    foreach (var imgPath in imagesToSend)
                    {
                        using (var content = new MultipartFormDataContent())
                        {
                            //content.Headers.ContentType = MediaTypeHeaderValue.Parse(FilesHelper.GetContentTypeByExtension(Path.GetExtension(imgPath)));
                            content.Add(new StreamContent(File.OpenRead(imgPath)), Path.GetFileNameWithoutExtension(imgPath),
                                Path.GetFileName(imgPath));
                            var respMsg = await httpClient.PostAsync(url, content);
                            var respJson = JObject.Parse(await respMsg.Content.ReadAsStringAsync());

                            var saveWallResp = await new PhotoSaveWallPhotoRequest(httpClient, vkInfo.AccessToken)
                            {
                                Server = respJson["server"].ToString(),
                                Photo = respJson["photo"].ToString(),
                                Hash = respJson["hash"].ToString(),
                                GroupId = vkGroupId
                            }.PerformAsync();
                            imagesToAdd.AddRange(saveWallResp.Select(x => $"photo{x.OwnerId}_{x.Id}"));
                        }
                    }
                    var wallPostResp = await new WallPostRequest(httpClient, vkInfo.AccessToken)
                    {
                        Attachments = string.Join(",", imagesToAdd),
                        OwnerId = vkGroupId,
                        Message = post.Description + "\n " + tagsStr
                    }.PerformAsync();
                }
                else
                {
                    var wallPostResp = await new WallPostRequest(httpClient, vkInfo.AccessToken)
                    {
                        Message = postMessage,
                        OwnerId = vkGroupId
                    }.PerformAsync();
                }
                return Success;
            }
            return DataServiceResult.Failed("Не найдены данные по ВК аккаунту");
        }

        public async Task<DataServiceResult> GetGroupIdAsync(string userId, string domain)
        {
            var vkInfo = await externalServices.GetAsync(userId, VkSettings.DefaultAuthType);
            if (vkInfo == null || string.IsNullOrEmpty(vkInfo.AccessToken))
                DataServiceResult.Failed("Токен пользователя не найден");
            try
            {
                var resp = await new GroupsGetByIdRequest(httpClient, vkInfo.AccessToken)
                {
                    GroupId = domain
                }.PerformAsync();
                var group = resp.FirstOrDefault();
                if (group == null)
                    return DataServiceResult.Failed("Группа не найдена");
                return DataServiceResult.Success(group.Id);
            }
            catch(VkApiException e)
            {
                logger.Warn("Ошибка при получении группы: " + e.Message);
                return DataServiceResult.Failed(e.Message);
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при получении id группы", e);
            }
        }
    }
}