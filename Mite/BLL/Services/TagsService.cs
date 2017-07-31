using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Mite.DAL.Infrastructure;
using Mite.Models;
using Mite.DAL.Entities;
using System.Linq;
using Mite.BLL.Core;
using System;
using Mite.BLL.DTO;
using Mite.DAL.Repositories;

namespace Mite.BLL.Services
{
    public interface ITagsService
    {
        Task<IEnumerable<string>> GetTagsByNameAsync(string name);
        /// <summary>
        /// Достаем все теги, доступные для пользователей(подтвержденные модераторами)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetForUser();
        Task<IEnumerable<Tag>> GetForModer();
        Task<IEnumerable<TagDTO>> GetWithPopularity(bool isConfirmed);
    }
    public class TagsService : DataService, ITagsService
    {
        public TagsService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<string>> GetTagsByNameAsync(string name)
        {
            var repo = Database.GetRepo<TagsRepository, Tag>();
            var tags = await repo.GetByNameAsync(name);

            return Mapper.Map<IEnumerable<string>>(tags);
        }
        public async Task<IEnumerable<string>> GetForUser()
        {
            var tags = await Database.GetRepo<TagsRepository, Tag>().GetAllAsync(true);
            return Mapper.Map<IEnumerable<string>>(tags);
        }

        public async Task<IEnumerable<Tag>> GetForModer()
        {
            var tags = await Database.GetRepo<TagsRepository, Tag>().GetAllAsync();
            return tags;
        }

        public Task<IEnumerable<TagDTO>> GetWithPopularity(bool isConfirmed)
        {
            return Database.GetRepo<TagsRepository, Tag>().GetAllWithPopularityAsync(isConfirmed);
        }
    }
}