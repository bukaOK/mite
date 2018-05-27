using Mite.BLL.Core;
using System;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using System.Threading.Tasks;
using Mite.Models;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using AutoMapper;
using NLog;
using System.Web.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Filters;
using Mite.CodeData.Enums;

namespace Mite.BLL.Services
{
    public interface IAuthorServiceService : IDataService
    {
        Task<DataServiceResult> AddVkListAsync(IList<VkServiceModel> services, string userId);
        Task<DataServiceResult> AddAsync(AuthorServiceModel model);
        Task<DataServiceResult> UpdateAsync(AuthorServiceModel model);
        /// <summary>
        /// Удаление услуги
        /// </summary>
        /// <param name="id">Id услуги</param>
        /// <param name="ownerId">Id автора-владельца</param>
        /// <returns></returns>
        Task<DataServiceResult> RemoveAsync(Guid id, string ownerId);
        Task<AuthorServiceModel> GetAsync(Guid id);
        Task<ServiceTopFilterModel> GetTopModelAsync(string userId);
        Task<IEnumerable<ProfileServiceModel>> GetTopAsync(ServiceTopFilterModel filter);
        Task<AuthorServiceModel> GetNew();
        Task<IEnumerable<ProfileServiceModel>> GetByUserAsync(string userName, SortFilter sort);
        /// <summary>
        /// Получить услугу для показа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AuthorServiceShowModel> GetShowAsync(Guid id);
        Task<IEnumerable<GalleryItemModel>> GetGalleryAsync(Guid id);
        Task<DataServiceResult> RecountReliabilityAsync(Guid id);
    }
    public class AuthorServiceService : DataService, IAuthorServiceService
    {
        private readonly AuthorServiceRepository repo;
        private readonly IAuthorServiceTypeService serviceTypeService;
        private readonly ICityService cityService;
        private readonly AppUserManager userManager;
        private readonly ICountryService countryService;

        public AuthorServiceService(IUnitOfWork database, ILogger logger, IAuthorServiceTypeService serviceTypeService, 
            ICityService cityService, AppUserManager userManager, ICountryService countryService) : base(database, logger)
        {
            repo = database.GetRepo<AuthorServiceRepository, AuthorService>();
            this.serviceTypeService = serviceTypeService;
            this.cityService = cityService;
            this.userManager = userManager;
            this.countryService = countryService;
        }

        public async Task<DataServiceResult> AddAsync(AuthorServiceModel model)
        {
            var authorService = Mapper.Map<AuthorService>(model);
            authorService.ImageSrc = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, model.ImageBase64);
            authorService.ImageSrc_50 = FilesHelper.ToVirtualPath(ImagesHelper.Resize(HostingEnvironment.MapPath(authorService.ImageSrc), 500));
            authorService.CreateDate = DateTime.UtcNow;

            try
            {
                await repo.AddAsync(authorService);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                FilesHelper.DeleteFile(authorService.ImageSrc);
                FilesHelper.DeleteFile(authorService.ImageSrc_50);

                logger.Error("Ошибка при попытке создать услугу: " + e.Message);
                return DataServiceResult.Failed("Ошибка при попытке создать услугу.");
            }
        }

        public async Task<AuthorServiceModel> GetAsync(Guid id)
        {
            var service = await repo.GetAsync(id);
            var model = Mapper.Map<AuthorServiceModel>(service);

            model.ServiceTypes = await serviceTypeService.GetSelectListAsync(service.ServiceTypeId);
            return model;
        }

        public async Task<AuthorServiceModel> GetNew()
        {
            return new AuthorServiceModel
            {
                ServiceTypes = await serviceTypeService.GetSelectListAsync(Guid.Empty)
            };
        }

        public async Task<IEnumerable<ProfileServiceModel>> GetByUserAsync(string userName, SortFilter sort)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                throw new ArgumentException("Неизвестное имя пользователя");
            var services = await repo.GetByUserAsync(user.Id, sort);
            return Mapper.Map<IEnumerable<ProfileServiceModel>>(services);
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id, string ownerId)
        {
            var authorService = await repo.GetAsync(id);
            if(!string.Equals(authorService.AuthorId, ownerId))
            {
                return DataServiceResult.Failed("Только владелец может удалить эту работу.");
            }
            var dealsCount = await repo.DealsCountAsync(id);
            if (dealsCount > 0)
                return DataServiceResult.Failed("Нельзя удалить услугу, по которой были осуществлены сделки.");
            try
            {
                await repo.RemoveAsync(id);
                return Success;
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при попытке удаления: " + e.Message);
                return DataServiceResult.Failed("Ошибка при попытке удаления");
            }
        }

        public async Task<DataServiceResult> UpdateAsync(AuthorServiceModel model)
        {
            var authorService = await repo.GetAsync(model.Id);
            Mapper.Map(model, authorService);

            if(!string.Equals(model.ImageBase64, authorService.ImageSrc, StringComparison.OrdinalIgnoreCase))
            {
                FilesHelper.DeleteFile(authorService.ImageSrc);
                authorService.ImageSrc = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, model.ImageBase64);

                FilesHelper.DeleteFile(authorService.ImageSrc_50);
                ImagesHelper.Compressed.Compress(HostingEnvironment.MapPath(authorService.ImageSrc));
                authorService.ImageSrc_50 = ImagesHelper.Compressed.CompressedVirtualPath(authorService.ImageSrc);
            }
            try
            {
                await repo.UpdateAsync(authorService);
                return DataServiceResult.Success();
            }
            catch(Exception e)
            {
                FilesHelper.DeleteFile(authorService.ImageSrc);
                FilesHelper.DeleteFile(authorService.ImageSrc_50);

                logger.Error("Ошибка при попытке обновить услугу автора: " + e.Message);
                return DataServiceResult.Failed("Ошибка при попытке обновить услугу автора.");
            }
        }

        public async Task<AuthorServiceShowModel> GetShowAsync(Guid id)
        {
            var authorService = await repo.GetWithServiceTypeAsync(id);

            if (authorService == null)
                return null;
            var model = Mapper.Map<AuthorServiceShowModel>(authorService);
            var feedbacks = await repo.GetFeedbacksAsync(id);
            model.Feedbacks = feedbacks.Count > 0
                ? Mapper.Map<IList<ServiceFeedbackModel>>(feedbacks)
                : new List<ServiceFeedbackModel>();
            return model;
        }

        public async Task<ServiceTopFilterModel> GetTopModelAsync(string userId)
        {
            var model = new ServiceTopFilterModel
            {
                Cities = await cityService.GetSelectListAsync(userId),
                Countries = await countryService.GetSelectListAsync(userId),
                ServiceTypes = await serviceTypeService.GetSelectListAsync(Guid.Empty)
            };
            var (min, max) = await repo.GetMinMaxPricesAsync();
            model.Min = (int)min;
            model.Max = (int)max;

            return model;
        }

        public async Task<IEnumerable<ProfileServiceModel>> GetTopAsync(ServiceTopFilterModel filterModel)
        {
            const int range = 20;

            var filter = Mapper.Map<ServiceTopFilter>(filterModel);
            filter.Range = range;
            filter.Offset = range * (filterModel.Page - 1);

            var result = await repo.GetByFilterAsync(filter);
            return Mapper.Map<IEnumerable<ProfileServiceModel>>(result);
        }

        public async Task<IEnumerable<GalleryItemModel>> GetGalleryAsync(Guid id)
        {
            var dealRepo = Database.GetRepo<DealRepository, Deal>();
            var items = await dealRepo.GetServiceGalleryAsync(id);
            return Mapper.Map<IEnumerable<GalleryItemModel>>(items);
        }

        public async Task<DataServiceResult> RecountReliabilityAsync(Guid id)
        {
            try
            {
                await repo.RecountReliabilityAsync(id, DealConstants.BadCoef, DealConstants.GoodCoef);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при пересчете надежности услуги", e);
            }
        }

        public async Task<DataServiceResult> AddVkListAsync(IList<VkServiceModel> serviceModels, string userId)
        {
            var services = Mapper.Map<IEnumerable<AuthorService>>(serviceModels, 
                opts => opts.Items.Add("UserId", userId));
            try
            {
                await repo.AddListAsync(services);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении", e);
            }
        }
    }
}