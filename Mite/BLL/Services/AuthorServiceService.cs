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

namespace Mite.BLL.Services
{
    public interface IAuthorServiceService : IDataService
    {
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
        Task<AuthorServiceModel> GetNew();
        Task<IEnumerable<ProfileServiceModel>> GetByUserAsync(string userId);
        /// <summary>
        /// Получить услугу для показа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AuthorServiceShowModel> GetShowAsync(Guid id);
    }
    public class AuthorServiceService : DataService, IAuthorServiceService
    {
        private readonly AuthorServiceRepository repo;
        private readonly ILogger logger;
        private readonly IAuthorServiceTypeService serviceTypeService;

        public AuthorServiceService(IUnitOfWork database, ILogger logger, IAuthorServiceTypeService serviceTypeService) : base(database)
        {
            repo = database.GetRepo<AuthorServiceRepository, AuthorService>();
            this.logger = logger;
            this.serviceTypeService = serviceTypeService;
        }

        public async Task<DataServiceResult> AddAsync(AuthorServiceModel model)
        {
            var authorService = Mapper.Map<AuthorService>(model);

            authorService.ImageSrc = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, model.ImageBase64);
            ImagesHelper.Compressed.Compress(HostingEnvironment.MapPath(authorService.ImageSrc));
            authorService.ImageSrc_50 = ImagesHelper.Compressed.CompressedVirtualPath(authorService.ImageSrc);

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

        public async Task<IEnumerable<ProfileServiceModel>> GetByUserAsync(string userId)
        {
            var services = await repo.GetByUserAsync(userId);
            return Mapper.Map<IEnumerable<ProfileServiceModel>>(services);
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id, string ownerId)
        {
            var authorService = await repo.GetAsync(id);
            if(!string.Equals(authorService.AuthorId, ownerId))
            {
                return DataServiceResult.Failed("Только владелец может удалить эту работу.");
            }
            try
            {
                await repo.RemoveAsync(id);
                return DataServiceResult.Success();
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
            return model;
        }
    }
}