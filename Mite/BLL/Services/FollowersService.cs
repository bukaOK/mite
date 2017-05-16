using Mite.BLL.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;

namespace Mite.BLL.Services
{
    public interface IFollowersService
    {
        /// <summary>
        /// Получить подписчиков пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowersByUserAsync(string userId);
        /// <summary>
        /// Получить пользователей, на которых текущий подписан
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowingsByUserAsync(string userId);
    }
    public class FollowersService : DataService, IFollowersService
    {
        public FollowersService(IUnitOfWork database) : base(database)
        {
        }

        public async Task<List<UserShortModel>> GetFollowersByUserAsync(string userId)
        {
            var followers = await Database.FollowersRepository.GetFollowersByUserAsync(userId);
            var fModels = Mapper.Map<List<UserShortModel>>(followers.Select(x => x.User));
            return fModels;
        }

        public async Task<List<UserShortModel>> GetFollowingsByUserAsync(string userId)
        {
            var followings = await Database.FollowersRepository.GetFollowingsByUserAsync(userId);
            var fModels = Mapper.Map<List<UserShortModel>>(followings.Select(x => x.FollowingUser));
            return fModels;
        }
    }
}