using Mite.BLL.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using AutoMapper;
using Mite.Helpers;
using System.Web.Hosting;
using Mite.BLL.DTO;
using Mite.BLL.IdentityManagers;

namespace Mite.BLL.Services
{
    public interface IFollowersService
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

            var followers = await Database.FollowersRepository.GetFollowersByUserAsync(user.Id);
            var fModels = Mapper.Map<List<UserShortModel>>(followers.Select(x => x.User));
            foreach(var follower in fModels)
            {
                var img = new ImageDTO(follower.AvatarSrc, imagesFolder);
                if (img.CompressedExists)
                {
                    follower.AvatarSrc = img.CompressedVirtualPath;
                }
            }
            return fModels;
        }

        public async Task<List<UserShortModel>> GetFollowingsByUserAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);

            var followings = await Database.FollowersRepository.GetFollowingsByUserAsync(user.Id);
            var fModels = Mapper.Map<List<UserShortModel>>(followings.Select(x => x.FollowingUser));
            foreach(var following in fModels)
            {
                var img = new ImageDTO(following.AvatarSrc, imagesFolder);
                if (img.CompressedExists)
                {
                    following.AvatarSrc = img.CompressedVirtualPath;
                }
            }
            return fModels;
        }
    }
}