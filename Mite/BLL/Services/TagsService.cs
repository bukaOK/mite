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
            var tags = await Database.TagsRepository.GetByNameAsync(name);

            return Mapper.Map<IEnumerable<string>>(tags);
        }
        public async Task<IEnumerable<string>> GetForUser()
        {
            var tags = await Database.TagsRepository.GetAllAsync(true);
            return Mapper.Map<IEnumerable<string>>(tags);
        }

        public async Task<IEnumerable<Tag>> GetForModer()
        {
            var tags = await Database.TagsRepository.GetAllAsync();
            return tags;
        }

        public Task<IEnumerable<TagDTO>> GetWithPopularity(bool isConfirmed)
        {
            return Database.TagsRepository.GetAllWithPopularityAsync(isConfirmed);
        }
    }
}