﻿using System;
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
using Mite.BLL.DTO;
using Mite.BLL.Helpers;

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

        Task<UserProfileDTO> GetUserProfileAsync(string name, string currentUserId);
        SocialLinksDTO GetSocialLinks(string userId);
        Task<SocialLinksDTO> GetSocialLinksAsync(string userId);
        Task UpdateSocialLinksAsync(SocialLinksDTO dto, string userId);
    }
    public class UserService : DataService, IUserService
    {
        private readonly AppUserManager userManager;
        private readonly AppSignInManager signInManager;
        private readonly ILogger logger;

        public UserService(AppUserManager userManager, AppSignInManager signInManager, IUnitOfWork unitOfWork,
            ILogger logger): base(unitOfWork)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        public async Task<SignInStatus> LoginAsync(UserLoginDTO dto)
        {
            var emailRegexPattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
            if (Regex.IsMatch(dto.UserName, emailRegexPattern, RegexOptions.IgnoreCase))
            {
                var user = await userManager.FindByEmailAsync(dto.UserName);
                dto.UserName = user.UserName;
            }
            return await signInManager.PasswordSignInAsync(dto.UserName, dto.Password, dto.Remember,
                    shouldLockout: true);
        }
        public Task<SignInStatus> LoginAsync(ExternalLoginInfo loginInfo, bool remember)
        {
            return signInManager.ExternalSignInAsync(loginInfo, remember);
        }
        public async Task<IdentityResult> RegisterAsync(UserRegisterDTO registerDto, ExternalLoginInfo loginInfo = null)
        {
            var external = loginInfo != null;

            var emailUser = await userManager.FindByEmailAsync(registerDto.Email);
            var nameUser = await userManager.FindByNameAsync(registerDto.UserName);
            if (emailUser != null || nameUser != null)
            {
                var errors = new List<string>();
                if (emailUser != null)
                    errors.Add("Пользователь с таким e-mail уже существует.");
                if (nameUser != null)
                    errors.Add("Пользователь с таким именем уже существует.");
                return new IdentityResult(errors);
            }

            var refid = registerDto.RefererId;
            if (refid != null)
            {
                var referer = await userManager.FindByIdAsync(refid);
                if (referer == null)
                    registerDto.RefererId = null;
            }

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                RegisterDate = DateTime.UtcNow,
                RefererId = registerDto.RefererId
            };
            var result = external
                ? await userManager.CreateAsync(user)
                : await userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                if (external)
                {
                    await userManager.AddLoginAsync(user.Id, loginInfo.Login);
                }
                await signInManager.SignInAsync(user, true, false);
                await userManager.AddToRoleAsync(user.Id, "user");
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

        public async Task<UserProfileDTO> GetUserProfileAsync(string name, string currentUserId)
        {
            var postsRepo = Database.GetRepo<PostsRepository, Post>();
            var followersRepo = Database.GetRepo<FollowersRepository, Follower>();

            var user = await userManager.FindByNameAsync(name);
            if (user == null)
                return null;
            var dto = Mapper.Map<UserProfileDTO>(user);

            dto.PostsCount = await postsRepo.GetPublishedPostsCount(user.Id);
            dto.FollowersCount = await followersRepo.GetFollowersCount(user.Id);

            if(user.Id != currentUserId)
            {
                dto.IsFollowing = await followersRepo.IsFollower(currentUserId, user.Id);
            }
            return dto;
        }

        public SocialLinksDTO GetSocialLinks(string userId)
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
            return Mapper.Map<SocialLinksDTO>(socialLinks);
        }

        public async Task<SocialLinksDTO> GetSocialLinksAsync(string userId)
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
            return Mapper.Map<SocialLinksDTO>(socialLinks);
        }

        public async Task UpdateSocialLinksAsync(SocialLinksDTO dto, string userId)
        {
            var repo = Database.GetRepo<SocialLinksRepository, SocialLinks>();

            var socialLinks = await repo.GetAsync(userId);
            if(socialLinks == null)
            {
                socialLinks = Mapper.Map<SocialLinks>(dto);
                socialLinks.UserId = userId;
                await repo.AddAsync(socialLinks);
            }
            else
            {
                socialLinks = Mapper.Map<SocialLinks>(dto);
                await repo.UpdateAsync(socialLinks);
            }
        }
    }
}