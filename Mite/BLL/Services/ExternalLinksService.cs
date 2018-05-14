using AutoMapper;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.BLL.Services
{
    public interface IExternalLinksService : IDataService
    {
        Task<DataServiceResult> AddAsync(IEnumerable<ExternalLinkModel> linkModels, string userId);
        Task<IEnumerable<ExternalLinkModel>> GetByUserForShowAsync(string userId);
        Task<IEnumerable<ExternalLinkEditModel>> GetByUserAsync(string userId);
        Task<bool> IsConfirmedAsync(string link);
    }
    public class ExternalLinksService : DataService, IExternalLinksService
    {
        private readonly ExternalLinksRepository linksRepository;

        public ExternalLinksService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            linksRepository = database.GetRepo<ExternalLinksRepository, ExternalLink>();
        }

        public async Task<DataServiceResult> AddAsync(IEnumerable<ExternalLinkModel> linkModels, string userId)
        {
            var links = Mapper.Map<IEnumerable<ExternalLink>>(linkModels, opts => opts.Items.Add("userId", userId));
            try
            {
                await linksRepository.AddOrUpdateAsync(links);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при добавлении внешних ссылок", e);
            }
        }

        public async Task<IEnumerable<ExternalLinkEditModel>> GetByUserAsync(string userId)
        {
            var links = await linksRepository.GetByUserAsync(userId);
            return Mapper.Map<IEnumerable<ExternalLinkEditModel>>(links);
        }

        public async Task<IEnumerable<ExternalLinkModel>> GetByUserForShowAsync(string userId)
        {
            var links = await linksRepository.GetByUserAsync(userId);
            return Mapper.Map<IEnumerable<ExternalLinkModel>>(links);
        }

        public async Task<bool> IsConfirmedAsync(string link)
        {
            var linkEntity = await linksRepository.GetByUrlAsync(link);
            return linkEntity?.Confirmed ?? false;
        }
    }
}