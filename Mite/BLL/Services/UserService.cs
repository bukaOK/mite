using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.Helpers;
using Mite.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using NLog;
using System.Text.RegularExpressions;
using Mite.DAL.Repositories;
using System.Web.Hosting;
using Mite.BLL.Helpers;
using Mite.CodeData;
using Mite.CodeData.Constants;

namespace Mite.BLL.Services
{
    public interface IUserService : IDataService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel registerModel, ExternalLoginInfo loginInfo = null);

        Task<SignInStatus> LoginAsync(LoginModel profileSettings);
        Task<SignInStatus> LoginAsync(ExternalLoginInfo loginInfo, bool remember);

        Task<IdentityResult> UpdateUserAsync(ProfileSettingsModel settings, string userId);

        /// <summary>
        /// Обновляем аватар пользователя
        /// </summary>
        /// <param name="imagesFolder">Путь к изображениям на сервере</param>
        /// <param name="imageBase64">Само изображение(в кодировке base64)</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        Task<IdentityResult> UpdateUserAvatarAsync(string imagesFolder, string imageBase64, string userId);
        Task<ProfileModel> GetUserProfileAsync(string name, string currentUserId);
        SocialLinksModel GetSocialLinks(string userId);
        Task<SocialLinksModel> GetSocialLinksAsync(string userId);
        Task UpdateSocialLinksAsync(SocialLinksModel model, string userId);
        /// <summary>
        /// Пересчитать надежность пользователя
        /// </summary>
        /// <returns></returns>
        Task<DataServiceResult> RecountReliabilityAsync(string userId);
    }
    public class UserService : DataService, IUserService
    {
        private readonly AppUserManager userManager;
        private readonly AppSignInManager signInManager;

        public UserService(AppUserManager userManager, AppSignInManager signInManager, IUnitOfWork unitOfWork,
            ILogger logger): base(unitOfWork, logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<SignInStatus> LoginAsync(LoginModel model)
        {
            var emailRegexPattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
            if (Regex.IsMatch(model.UserName, emailRegexPattern, RegexOptions.IgnoreCase))
            {
                var user = await userManager.FindByEmailAsync(model.UserName);
                model.UserName = user.UserName;
            }
            return await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.Remember,
                    shouldLockout: true);
        }
        public Task<SignInStatus> LoginAsync(ExternalLoginInfo loginInfo, bool remember)
        {
            return signInManager.ExternalSignInAsync(loginInfo, remember);
        }
        public async Task<IdentityResult> RegisterAsync(RegisterModel registerModel, ExternalLoginInfo loginInfo = null)
        {
            var external = loginInfo != null;

            var emailUser = await userManager.FindByEmailAsync(registerModel.Email);
            var nameUser = await userManager.FindByNameAsync(registerModel.UserName);
            if (emailUser != null || nameUser != null)
            {
                var errors = new List<string>();
                if (emailUser != null)
                    errors.Add("Пользователь с таким e-mail уже существует.");
                if (nameUser != null)
                    errors.Add("Пользователь с таким именем уже существует.");
                return new IdentityResult(errors);
            }

            var refid = registerModel.RefererId;
            if (refid != null)
            {
                var referer = await userManager.FindByIdAsync(refid);
                if (referer == null)
                    registerModel.RefererId = null;
            }

            var user = new User
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
                RegisterDate = DateTime.UtcNow,
                RefererId = registerModel.RefererId
            };
            var result = external
                ? await userManager.CreateAsync(user)
                : await userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                if (external)
                {
                    result = await userManager.AddLoginAsync(user.Id, loginInfo.Login);
                }
                await signInManager.SignInAsync(user, true, false);
                switch ((RegisterRoles?)registerModel.RegisterRole)
                {
                    case RegisterRoles.Author:
                        result = await userManager.AddToRoleAsync(user.Id, RoleNames.Author);
                        break;
                    case RegisterRoles.Client:
                        result = await userManager.AddToRoleAsync(user.Id, RoleNames.Client);
                        break;
                    default:
                        goto case RegisterRoles.Author;
                }
            }
            return result;
        }
        public async Task<IdentityResult> UpdateUserAsync(ProfileSettingsModel settings, string userId)
        {
            var existingUser = await userManager.FindByIdAsync(userId);
            var updatedUser = Mapper.Map(settings, existingUser);
            //Проверяем, есть ли пользователь с таким именем
            var userByName = await userManager.FindByNameAsync(settings.NickName);
            if(userByName != null && userByName.Id != existingUser.Id)
            {
                return new IdentityResult("Пользователь с таким ником уже существует");
            }
            var result = await userManager.UpdateAsync(updatedUser);

            return result;
        }

        public async Task<IdentityResult> UpdateUserAvatarAsync(string imagesFolder, string imageBase64, string userId)
        {
            string imagePath;
            string fullPath;
            try
            {
                imagePath = FilesHelper.CreateImage(imagesFolder, imageBase64);
                fullPath = HostingEnvironment.MapPath(imagePath);
                ImagesHelper.Compressed.Compress(fullPath);
            }
            catch (FormatException)
            {
                return IdentityResult.Failed("Изображение имеет неверный формат");
            }

            var existingUser = await userManager.FindByIdAsync(userId);
            var existingAvatarSrc = existingUser.AvatarSrc;
            var existingAvatarFolders = existingAvatarSrc.Split('/');
            existingUser.AvatarSrc = imagePath;
            if(existingAvatarSrc != null && existingAvatarFolders[1] == "Public")
                FilesHelper.DeleteFile(existingAvatarSrc);

            //удаляем сжатую копию старой аватарки
            var fullAvatarSrc = HostingEnvironment.MapPath(existingAvatarSrc);
            if (ImagesHelper.Compressed.CompressedExists(fullAvatarSrc))
            {
                FilesHelper.DeleteFileFull(ImagesHelper.Compressed.CompressedPath(fullAvatarSrc, "jpg"));
            }
            var result = await userManager.UpdateAsync(existingUser);
            return result;
        }

        public async Task<ProfileModel> GetUserProfileAsync(string name, string currentUserId)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var postsRepo = Database.GetRepo<PostsRepository, Post>();
            var followersRepo = Database.GetRepo<FollowersRepository, Follower>();

            var user = await userManager.FindByNameAsync(name);
            if (user == null)
                return null;
            var userModel = Mapper.Map<ProfileModel>(user);
            userModel.SocialLinks = await GetSocialLinksAsync(user.Id);
            userModel.IsAuthor = await userManager.IsInRoleAsync(user.Id, RoleNames.Author);

            if (userModel.IsAuthor)
            {
                userModel.PostsCount = await postsRepo.GetPublishedPostsCount(user.Id);
                userModel.FollowersCount = await followersRepo.GetFollowersCount(user.Id);
            }
            else
            {
                userModel.FollowingsCount = await followersRepo.GetFollowingsCountAsync(user.Id);
            }
            if(user.Id != currentUserId)
            {
                userModel.IsFollowing = await followersRepo.IsFollower(currentUserId, user.Id);
            }
            return userModel;
        }

        public SocialLinksModel GetSocialLinks(string userId)
        {
            var repo = Database.GetRepo<SocialLinksRepository, SocialLinks>();
            var socialLinks = repo.Get(userId);
            if (socialLinks == null)
            {
                socialLinks = new SocialLinks
                {
                    UserId = userId
                };
            }
            return Mapper.Map<SocialLinksModel>(socialLinks);
        }

        public async Task<SocialLinksModel> GetSocialLinksAsync(string userId)
        {
            var repo = Database.GetRepo<SocialLinksRepository, SocialLinks>();

            var socialLinks = await repo.GetAsync(userId);
            if (socialLinks == null)
            {
                socialLinks = new SocialLinks
                {
                    UserId = userId
                };
            }
            return Mapper.Map<SocialLinksModel>(socialLinks);
        }

        public async Task UpdateSocialLinksAsync(SocialLinksModel model, string userId)
        {
            var repo = Database.GetRepo<SocialLinksRepository, SocialLinks>();

            var socialLinks = await repo.GetAsync(userId);
            if(socialLinks == null)
            {
                socialLinks = Mapper.Map<SocialLinks>(model);
                socialLinks.UserId = userId;
                await repo.AddAsync(socialLinks);
            }
            else
            {
                Mapper.Map(model, socialLinks);
                await repo.UpdateAsync(socialLinks);
            }
        }

        public async Task<DataServiceResult> RecountReliabilityAsync(string userId)
        {
            var repo = Database.GetRepo<UserRepository, User>();
            try
            {
                await repo.RecountReliabilityAsync(userId, DealConstants.GoodCoef, DealConstants.BadCoef);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Ошибка при пересчете", e);
            }
        }
    }
}