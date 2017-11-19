using Mite.BLL.Core;
using Mite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using System.Web.Hosting;
using Mite.BLL.IdentityManagers;
using Mite.DAL.Repositories;
using Mite.DAL.Entities;
using Mite.BLL.Helpers;
using System;
using Mite.CodeData.Enums;
using NLog;
using Mite.CodeData.Constants.Automapper;

namespace Mite.BLL.Services
{
    public interface IFollowersService : IDataService
    {
        /// <summary>
        /// Получить подписчиков пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowersByUserAsync(string userName, SortFilter sort);
        /// <summary>
        /// Получить пользователей, на которых текущий подписан
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowingsByUserAsync(string userName, SortFilter sort);
        Task<int> GetFollowersCountAsync(string userId);
    }
    public class FollowersService : DataService, IFollowersService
    {
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly AppUserManager userManager;

        public FollowersService(IUnitOfWork database, AppUserManager userManager, ILogger logger) : base(database, logger)
        {
            this.userManager = userManager;
        }

        public async Task<List<UserShortModel>> GetFollowersByUserAsync(string userName, SortFilter sort)
        {
            var user = await userManager.FindByNameAsync(userName);
            var repo = Database.GetRepo<FollowersRepository, Follower>();

            var followers = await repo.GetFollowersByUserAsync(user.Id, sort);
            var fModels = Mapper.Map<List<UserShortModel>>(followers.Select(x => x.User), opts =>
            {
                opts.Items.Add(UserProfileConstants.MaxNameLength, 13);
                opts.Items.Add(UserProfileConstants.MaxAboutLength, 70);
            });

            return fModels;
        }

        public Task<int> GetFollowersCountAsync(string userId)
        {
            return Database.GetRepo<FollowersRepository, Follower>().GetFollowersCount(userId);
        }

        public async Task<List<UserShortModel>> GetFollowingsByUserAsync(string userName, SortFilter sort)
        {
            var user = await userManager.FindByNameAsync(userName);
            var repo = Database.GetRepo<FollowersRepository, Follower>();

            var followings = await repo.GetFollowingsByUserAsync(user.Id, sort);
            var fModels = Mapper.Map<List<UserShortModel>>(followings.Select(x => x.FollowingUser), opts =>
            {
                opts.Items.Add(UserProfileConstants.MaxNameLength, 13);
                opts.Items.Add(UserProfileConstants.MaxAboutLength, 70);
            });

            return fModels;
        }
    }
}