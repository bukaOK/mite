using Mite.BLL.Core;
using Mite.DAL.Entities;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.DAL.Repositories;
using System;
using Mite.CodeData.Constants;
using System.Net.Http;
using System.Web.Hosting;
using Mite.CodeData.Enums;
using Mite.BLL.Helpers;
using System.Collections.Generic;
using System.Linq;
using Mite.BLL.IdentityManagers;
using System.Web;

namespace Mite.BLL.Services
{
    public interface IExternalServices : IDataService
    {
        /// <summary>
        /// Добавляет внешний сервис пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="serviceType">Тип сервиса(Google, Facebook, etc.)</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task AddAsync(string userId, string serviceType, string accessToken);
        Task<ExternalService> GetAsync(string userId, string serviceName);
        /// <summary>
        /// Получить все внешние сервисы пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<ExternalService>> GetAsync(string userId);
        /// <summary>
        /// Обновляет сервис
        /// </summary>
        /// <param name="providerKey">Id пользователя во внешнем сервисе</param>
        /// <param name="serviceType"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<DataServiceResult> UpdateAsync(string providerKey, string serviceType, string accessToken);
        void Remove(string userId, string serviceName);
        Task RemoveAsync(string userId, string serviceName);
        /// <summary>
        /// Опубликовать работу на внешних сервисах(vk, facebook, etc.)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddPostToServicesAsync(Guid postId, string userId);
        //Task<ESPublishSetting> GetUserPublishSettings(string userId);
    }
    public class ExternalServices : DataService, IExternalServices
    {
        private readonly HttpClient httpClient;
        private readonly AppUserManager userManager;
        private readonly ITwitterService twitterService;
        private readonly IVkService vkService;
        private readonly IFacebookService facebookService;
        private readonly IDeviantArtService deviantArtService;
        private readonly ExternalServiceRepository repo;

        private HttpRequest Request => HttpContext.Current.Request;

        public ExternalServices(IUnitOfWork database, ILogger logger, HttpClient httpClient, AppUserManager userManager,
            ITwitterService twitterService, IVkService vkService, IFacebookService facebookService, IDeviantArtService deviantArtService) : base(database, logger)
        {
            repo = database.GetRepo<ExternalServiceRepository, ExternalService>();
            this.httpClient = httpClient;
            this.userManager = userManager;
            this.twitterService = twitterService;
            this.vkService = vkService;
            this.facebookService = facebookService;
            this.deviantArtService = deviantArtService;
        }

        public async Task AddAsync(string userId, string serviceType, string accessToken)
        {
            var existingService = await repo.GetAsync(userId, serviceType);
            if (existingService == null)
            {
                var service = new ExternalService
                {
                    AccessToken = accessToken,
                    Name = serviceType,
                    UserId = userId
                };
                await repo.AddAsync(service);
            }
            else
            {
                existingService.AccessToken = accessToken;
                await repo.UpdateAsync(existingService);
            }
        }

        public async Task<DataServiceResult> AddPostToServicesAsync(Guid postId, string userId)
        {
            var domain = $"{Request.Url.Scheme}://{Request.Url.Host}";

            var postsRepository = Database.GetRepo<PostsRepository, Post>();
            var post = await postsRepository.GetFullAsync(postId);
            
            if (post.UserId != userId)
                return DataServiceResult.Failed("Неизвестный пользователь");

            var tagsStr = string.Join(" ", post.Tags.Select(x => "#" + x.Name));
            var postMessage = post.ContentType == PostContentTypes.Document
                ? await FilesHelper.ReadDocumentAsync(HostingEnvironment.MapPath(post.Content), 200)
                        + $"\n С полным текстом можно ознакомиться здесь: {domain}/posts/showpost/{post.Id.ToString("N")}\n {tagsStr}"
                : post.Description + "\n " + tagsStr;

            var esPublishSetting = await Database.GetRepo<ESPublishSettingsRepository, ESPublishSetting>().GetAsync(userId);            

            var imagesToSend = new List<string>();
            if(post.ContentType != PostContentTypes.Document)
            {
                imagesToSend.Add(HostingEnvironment.MapPath(post.Content));
                if (post.Collection != null && post.Collection.Count > 0)
                {
                    imagesToSend.AddRange(post.Collection.Select(x => HostingEnvironment.MapPath(x.ContentSrc)));
                }
            }
            await vkService.AddPostAsync(post, esPublishSetting.VkGroupId, postMessage, imagesToSend, tagsStr);
            //await facebookService.AddPostAsync(post, esPublishSetting.FacebookPageId, postMessage, imagesToSend, domain, tagsStr);
            await twitterService.AddPostAsync(post, postMessage, imagesToSend, tagsStr);
            await deviantArtService.AddPostAsync(post, domain);
            return Success;
        }

        public Task<ExternalService> GetAsync(string userId, string serviceName)
        {
            return repo.GetAsync(userId, serviceName);
        }
        public void Remove(string userId, string serviceName)
        {
            repo.Remove(userId, serviceName);
        }
        public Task RemoveAsync(string userId, string serviceName)
        {
            return repo.RemoveAsync(userId, serviceName);
        }

        public async Task<DataServiceResult> UpdateAsync(string providerKey, string serviceType, string accessToken)
        {
            var existingService = await repo.GetByProviderAsync(providerKey, serviceType);
            if(existingService == null)
            {
                logger.Error("Не найден внешний сервис");
                return DataServiceResult.Failed("Не найден внешний сервис");
            }
            existingService.AccessToken = accessToken;
            await repo.UpdateAsync(existingService);
            return DataServiceResult.Success();
        }

        public async Task<IEnumerable<ExternalService>> GetAsync(string userId)
        {
            var services = await repo.GetByUserAsync(userId);
            return services;
        }

        //public Task<ESPublishSetting> GetUserPublishSettings(string userId)
        //{
            
        //}
    }
}