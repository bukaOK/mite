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
using System.Text.RegularExpressions;
using Mite.BLL.DTO;

namespace Mite.BLL.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel registerModel);

        Task<SignInStatus> LoginAsync(LoginModel profileSettings);

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
    }
    public class UserService : DataService, IUserService
    {
        private readonly AppUserManager _userManager;
        private readonly AppSignInManager _signInManager;

        public UserService(AppUserManager userManager, AppSignInManager signInManager, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInStatus> LoginAsync(LoginModel model)
        {
            return await
                _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.Remember,
                    shouldLockout: true);
        }
        public async Task<IdentityResult> RegisterAsync(RegisterModel registerModel)
        {
            var user = new User
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
                RegisterDate = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, true, false);
                await _userManager.AddToRoleAsync(user.Id, "user");
            }
            return result;
        }
        
        public async Task<IdentityResult> UpdateUserAsync(ProfileSettingsModel settings, string userId)
        {
            var existingUser = await _userManager.FindByIdAsync(userId);
            var updatedUser = Mapper.Map(settings, existingUser);
            //Проверяем, есть ли пользователь с таким именем
            var userByName = await _userManager.FindByNameAsync(settings.NickName);
            if(userByName != null && userByName.Id != existingUser.Id)
            {
                return new IdentityResult("Пользователь с таким ником уже существует");
            }
            var result = await _userManager.UpdateAsync(updatedUser);

            return result;
        }

        public async Task<IdentityResult> UpdateUserAvatarAsync(string imagesFolder, string imageBase64, string userId)
        {
            string imagePath;
            try
            {
                imagePath = FilesHelper.CreateImage(imagesFolder, imageBase64);
                //Создаем сжатую копию аватарки
                var img = new ImageDTO(imagePath, imagesFolder);
                img.Compress();
            }
            catch (FormatException)
            {
                return IdentityResult.Failed("Изображение имеет неверный формат");
            }

            var existingUser = await _userManager.FindByIdAsync(userId);
            var existingAvatarSrc = existingUser.AvatarSrc;
            var existingAvatarFolders = existingAvatarSrc.Split('/');
            existingUser.AvatarSrc = imagePath;
            if(existingAvatarSrc != null && existingAvatarFolders[1] == "Public")
                FilesHelper.DeleteFile(existingAvatarSrc);

            //Создаем объект изображения и удаляем сжатую копию старой аватарки
            var existingImg = new ImageDTO(existingAvatarSrc, imagesFolder);
            if (existingImg.IsCompressedExists)
            {
                FilesHelper.DeleteFile(existingImg.CompressedVirtualPath);
            }
            var result = await _userManager.UpdateAsync(existingUser);
            return result;
        }

        public async Task<ProfileModel> GetUserProfileAsync(string name, string currentUserId)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user == null)
                return null;
            var userModel = Mapper.Map<ProfileModel>(user);

            userModel.PostsCount = await Database.PostsRepository.GetPublishedPostsCount(user.Id);
            userModel.FollowersCount = await Database.FollowersRepository.GetFollowersCount(user.Id);

            if(user.Id != currentUserId)
            {
                userModel.IsFollowing = await Database.FollowersRepository.IsFollower(currentUserId, user.Id);
            }
            return userModel;
        }
    }
}