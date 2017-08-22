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

namespace Mite.BLL.Services
{
    public interface IFollowersService : IDataService
    {
        /// <summary>
        /// Получить подписчиков пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowersByUserAsync(string userName);
        /// <summary>
        /// Получить пользователей, на которых текущий подписан
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<UserShortModel>> GetFollowingsByUserAsync(string userName);
        Task<int> GetFollowersCountAsync(string userId);
    }
    public class FollowersService : DataService, IFollowersService
    {
        private readonly string imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
        private readonly AppUserManager userManager;

        public FollowersService(IUnitOfWork database, AppUserManager userManager) : base(database)
        {
            this.userManager = userManager;
        }

        public async Task<List<UserShortModel>> GetFollowersByUserAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            var repo = Database.GetRepo<FollowersRepository, Follower>();

            var followers = await repo.GetFollowersByUserAsync(user.Id);
            var fModels = Mapper.Map<List<UserShortModel>>(followers.Select(x => x.User));
            foreach(var follower in fModels)
            {
                var fullPath = HostingEnvironment.MapPath(follower.AvatarSrc);
                if (ImagesHelper.Compressed.CompressedExists(fullPath))
                {
                    follower.AvatarSrc = ImagesHelper.Compressed.CompressedVirtualPath(fullPath);
                }
            }
            return fModels;
        }

        public Task<int> GetFollowersCountAsync(string userId)
        {
            return Database.GetRepo<FollowersRepository, Follower>().GetFollowersCount(userId);
        }

        public async Task<List<UserShortModel>> GetFollowingsByUserAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            var repo = Database.GetRepo<FollowersRepository, Follower>();

            var followings = await repo.GetFollowingsByUserAsync(user.Id);
            var fModels = Mapper.Map<List<UserShortModel>>(followings.Select(x => x.FollowingUser));
            foreach(var following in fModels)
            {
                var fullPath = HostingEnvironment.MapPath(following.AvatarSrc);
                if (ImagesHelper.Compressed.CompressedExists(fullPath))
                {
                    following.AvatarSrc = ImagesHelper.Compressed.CompressedVirtualPath(fullPath);
                }
            }
            return fModels;
        }
    }
}