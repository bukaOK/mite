using AutoMapper;
using Mite.BLL.Core;
using Mite.BLL.Helpers;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.BLL.Services
{
    public interface ICharacterService : IDataService
    {
        Task<CharacterModel> GetAsync(Guid id);
        Task<DataServiceResult> AddAsync(CharacterModel model);
        Task<DataServiceResult> UpdateAsync(CharacterModel model);
        Task<DataServiceResult> RemoveAsync(Guid id);
        Task<IEnumerable<ProfileCharacterModel>> GetByFilterAsync(string currentUserId, CharacterTopFilterModel filter);
        Task<IEnumerable<SelectListItem>> GetForPostAsync(string userId);
    }
    public class CharacterService : DataService, ICharacterService
    {
        private readonly CharacterRepository repository;
        private readonly AppUserManager userManager;

        public CharacterService(IUnitOfWork database, ILogger logger, AppUserManager userManager) : base(database, logger)
        {
            repository = database.GetRepo<CharacterRepository, Character>();
            this.userManager = userManager;
        }

        public async Task<DataServiceResult> AddAsync(CharacterModel model)
        {
            var character = Mapper.Map<Character>(model);
            
            try
            {
                character.ImageSrc = FilesHelper.CreateImage(PathConstants.PublicVirtualImageFolder, model.ImageSrc);

                if (!string.IsNullOrWhiteSpace(model.Description))
                {
                    character.DescriptionSrc = FilesHelper.CreateDocument(PathConstants.VirtualCharacterDocumentsFolder, model.Description);
                }
                await repository.AddAsync(character);

                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении персонажа", e);
            }
        }

        public async Task<CharacterModel> GetAsync(Guid id)
        {
            var character = await repository.GetAsync(id);
            if (character == null)
                return null;
            var model = Mapper.Map<CharacterModel>(character);
            model.Description = await FilesHelper.ReadDocumentAsync(character.DescriptionSrc);
            return model;
        }

        public async Task<IEnumerable<ProfileCharacterModel>> GetByFilterAsync(string currentUserId, CharacterTopFilterModel filterModel)
        {
            var filter = Mapper.Map<CharacterTopFilter>(filterModel);
            filter.UserId = currentUserId;
            var characters = await repository.GetByFilterAsync(filter);
            return Mapper.Map<IEnumerable<ProfileCharacterModel>>(characters);
        }

        public async Task<IEnumerable<SelectListItem>> GetForPostAsync(string userId)
        {
            var chars = await repository.GetForPostAsync(userId);
            return Mapper.Map<IEnumerable<SelectListItem>>(chars);
        }

        public async Task<DataServiceResult> RemoveAsync(Guid id)
        {
            try
            {
                var character = await repository.GetAsync(id);
                FilesHelper.DeleteFile(character.ImageSrc);

                if(!string.IsNullOrEmpty(character.DescriptionSrc))
                    FilesHelper.DeleteFile(character.DescriptionSrc);

                await repository.RemoveAsync(character);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при удалении персонажа", e);
            }
        }

        public async Task<DataServiceResult> UpdateAsync(CharacterModel model)
        {
            if (model.Id == null)
                return DataServiceResult.Failed("Неизвестный персонаж");
            try
            {
                var character = await repository.GetAsync(model.Id);
                if(!string.IsNullOrEmpty(model.ImageSrc) && character.ImageSrc != model.ImageSrc)
                {
                    if(Regex.IsMatch(model.ImageSrc, @"^https?:\/\/"))
                    {
                        FilesHelper.DeleteFile(character.ImageSrc);
                        character.ImageSrc = model.ImageSrc;
                    }
                    else if (FilesHelper.IsBase64(model.ImageSrc))
                    {
                        FilesHelper.DeleteFile(character.ImageSrc);
                        character.ImageSrc = FilesHelper.CreateImage(PathConstants.PublicVirtualImageFolder, model.ImageSrc);
                    }
                }
                if (!string.IsNullOrWhiteSpace(model.Description))
                    FilesHelper.UpdateDocument(character.DescriptionSrc, model.Description);

                if (model.Features != null && model.Features.Count > 0)
                    character.Features = Mapper.Map<List<CharacterFeature>>(model.Features);
                else
                    character.Features = null;

                character.Name = model.Name;
                character.Original = model.Original;
                character.UserId = model.UserId;
                await repository.UpdateAsync(character);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при обновлении персонажа", e);
            }
        }
    }
}