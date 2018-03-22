using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Mite.DAL.Infrastructure;
using Mite.DAL.Entities;
using Mite.BLL.Core;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System.Linq;

namespace Mite.BLL.Services
{
    public interface ITagsService : IDataService
    {
        Task<IEnumerable<string>> GetTagsByNameAsync(string name);
        /// <summary>
        /// Достаем все теги, доступные для пользователей(подтвержденные модераторами)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetForUserAsync();
        Task<IEnumerable<Tag>> GetForModerAsync();
        Task<IEnumerable<TagModel>> GetWithPopularityAsync(bool isConfirmed, int count);
        Task<IEnumerable<UserTagModel>> GetForUserTagsAsync(string userId);
    }
    public class TagsService : DataService, ITagsService
    {
        public TagsService(IUnitOfWork unitOfWork, ILogger logger) : base(unitOfWork, logger)
        {
        }

        public async Task<IEnumerable<string>> GetTagsByNameAsync(string name)
        {
            var repo = Database.GetRepo<TagsRepository, Tag>();
            var tags = await repo.GetByNameAsync(name);

            return Mapper.Map<IEnumerable<string>>(tags);
        }
        public async Task<IEnumerable<string>> GetForUserAsync()
        {
            var tags = await Database.GetRepo<TagsRepository, Tag>().GetAllAsync(true);
            return Mapper.Map<IEnumerable<string>>(tags);
        }

        public async Task<IEnumerable<Tag>> GetForModerAsync()
        {
            var tags = await Database.GetRepo<TagsRepository, Tag>().GetAllAsync();
            return tags;
        }

        public async Task<IEnumerable<TagModel>> GetWithPopularityAsync(bool isConfirmed, int count)
        {
            var tags = await Database.GetRepo<TagsRepository, Tag>().GetAllWithPopularityAsync(isConfirmed, count);
            return Mapper.Map<IEnumerable<TagModel>>(tags);
        }

        public async Task<IEnumerable<UserTagModel>> GetForUserTagsAsync(string userId)
        {
            var tagsRepo = Database.GetRepo<TagsRepository, Tag>();
            var tags = await tagsRepo.GetAllWithPopularityAsync(true, 500);
            var userTags = await tagsRepo.GetForUserAsync(userId);
            return tags.Select(x => new UserTagModel
            {
                Id = x.Id,
                Name = x.Name,
                IsFollower = userTags.Any(y => y.Id == x.Id)
            });
        }
    }
}