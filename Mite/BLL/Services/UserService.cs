using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
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
using System.Web;
using Mite.ExternalServices.VkApi.Groups;
using System.Net.Http;
using System.Linq;
using Mite.ExternalServices.VkApi.Core;

namespace Mite.BLL.Services
{
    public interface IUserService : IDataService
    {
        Task<IdentityResult> RegisterAsync(RegisterModel registerModel, ExternalLoginInfo loginInfo = null);

        Task<SignInStatus> LoginAsync(LoginModel profileSettings);
        Task<SignInStatus> LoginAsync(ExternalLoginInfo loginInfo, bool remember);

        Task<IdentityResult> UpdateUserAsync(ProfileSettingsModel settings, string userId);
        Task<DataServiceResult> UpdateUserAsync(NotifySettingsModel model, string userId);
        /// <summary>
        /// Генерирует код для приглашения
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> GenerateInviteAsync(string userId);
        /// <summary>
        /// Обновляем аватар пользователя
        /// </summary>
        /// <param name="imagesFolder">Путь к изображениям на сервере</param>
        /// <param name="imageBase64">Само изображение(в кодировке base64)</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        Task<IdentityResult> UpdateUserAvatarAsync(string imagesFolder, HttpPostedFileBase img, string userId);
        Task<ProfileModel> GetUserProfileAsync(string name, string currentUserId);
        LandingModel GetLandingModel();
        /// <summary>
        /// Пересчитать надежность пользователя
        /// </summary>
        /// <returns></returns>
        Task<DataServiceResult> RecountReliabilityAsync(string userId);
        /// <summary>
        /// Лучшие авторы за день
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserShortModel>> GetDayBestAsync();
    }
    public class UserService : DataService, IUserService
    {
        private readonly AppUserManager userManager;
        private readonly AppSignInManager signInManager;
        private readonly IExternalLinksService linksService;
        private readonly HttpClient client;

        public UserService(AppUserManager userManager, AppSignInManager signInManager, IUnitOfWork unitOfWork,
            ILogger logger, IExternalLinksService linksService, HttpClient client): base(unitOfWork, logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.linksService = linksService;
            this.client = client;
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
            if(registerModel.RegisterRole == null)
                return new IdentityResult(new[] { "Выберите тип пользователя" });
            
            var user = new User
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
                RegisterDate = DateTime.UtcNow,
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
                await signInManager.SignInAsync(user, true, false);
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

        public async Task<IdentityResult> UpdateUserAvatarAsync(string imagesFolder, HttpPostedFileBase img, string userId)
        {
            string vPath;
            try
            {
                //Раньше создавался оригинал и сжатая копия, сейчас только фиксированный размер
                vPath = ImagesHelper.Create(imagesFolder, img, 150);
            }
            catch (FormatException)
            {
                return IdentityResult.Failed("Изображение имеет неверный формат");
            }

            var existingUser = await userManager.FindByIdAsync(userId);
            var existingAvatarSrc = existingUser.AvatarSrc ?? PathConstants.AvatarSrc;
            var existingAvatarFolders = existingAvatarSrc.Split('/');
            if(existingAvatarSrc != null && existingAvatarFolders[1] == "Public")
                FilesHelper.DeleteFile(existingAvatarSrc);

            //удаляем сжатую копию старой аватарки
            var fullAvatarSrc = HostingEnvironment.MapPath(existingAvatarSrc);
            if (ImagesHelper.Compressed.CompressedExists(fullAvatarSrc))
            {
                FilesHelper.DeleteFileFull(ImagesHelper.Compressed.CompressedPath(fullAvatarSrc, "jpg"));
            }
            existingUser.AvatarSrc = vPath;
            var result = await userManager.UpdateAsync(existingUser);
            return result;
        }

        public async Task<ProfileModel> GetUserProfileAsync(string name, string currentUserId)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var postsRepo = Database.GetRepo<PostsRepository, Post>();
            var followersRepo = Database.GetRepo<FollowersRepository, Follower>();
            var blackListRepo = Database.GetRepo<BlackListUserRepository, BlackListUser>();
            var tariffRepo = Database.GetRepo<ClientTariffRepository, ClientTariff>();

            var user = await userManager.FindByNameAsync(name);
            if (user == null)
                return null;
            var userModel = Mapper.Map<ProfileModel>(user);
            userModel.ExternalLinks = await linksService.GetByUserForShowAsync(user.Id);
            userModel.IsAuthor = await userManager.IsInRoleAsync(user.Id, RoleNames.Author);

            var userCity = await Database.GetRepo<CitiesRepository, City>().GetWithCountryAsync(user.CityId);
            if (userCity != null)
                userModel.PlaceName = $"{userCity.Country.Name}, {userCity.Name}";

            if (userModel.IsAuthor)
            {
                userModel.PostsCount = await postsRepo.GetPublishedPostsCount(user.Id);
                userModel.FollowersCount = await followersRepo.GetFollowersCount(user.Id);
                userModel.SponsorsCount = await tariffRepo.GetSponsorsCountAsync(user.Id);
            }
            else
            {
                userModel.FollowingsCount = await followersRepo.GetFollowingsCountAsync(user.Id);
            }
            //Получаем инфу по группе пользователя
            var externalServiceRepo = Database.GetRepo<ExternalServiceRepository, ExternalService>();
            //Инфа есть, если пользователь заходил через вк
            var vkInfo = await externalServiceRepo.GetAsync(user.Id, VkSettings.DefaultAuthType);

            if(vkInfo != null)
            {
                if (string.IsNullOrEmpty(vkInfo.GroupId))
                {
                    var vkLink = (await Database.GetRepo<ExternalLinksRepository, ExternalLink>().GetByUserAsync(user.Id))
                        ?.FirstOrDefault(x => Regex.IsMatch(x.Url, @"vk\.com\/"))?.Url;
                    if (!string.IsNullOrEmpty(vkLink))
                    {
                        var groupDomain = Regex.Match(vkLink, @"vk\.com\/(?<groupDomain>.+)$").Groups["groupDomain"].Value;
                        var token = vkInfo.AccessToken ?? VkSettings.GroupKey;

                        try
                        {
                            var groupResp = await new GroupsGetByIdRequest(client, token)
                            {
                                GroupId = groupDomain
                            }.PerformAsync();
                            userModel.VkGroupId = groupResp.FirstOrDefault()?.Id;
                        }
                        catch (VkApiException e)
                        {
                            if (token != VkSettings.GroupKey)
                            {
                                try
                                {
                                    var groupResp = await new GroupsGetByIdRequest(client, VkSettings.GroupKey)
                                    {
                                        GroupId = groupDomain
                                    }.PerformAsync();
                                    userModel.VkGroupId = groupResp.FirstOrDefault()?.Id;
                                }
                                catch (VkApiException e1)
                                {
                                    logger.Warn("Ошибка при попытке получения группы: " + e1.Message);
                                }
                            }
                            else
                                logger.Warn("Ошибка при попытке получения группы: " + e.Message);
                        }
                    }
                }
                else
                    userModel.VkGroupId = vkInfo.GroupId;

                if (!string.IsNullOrEmpty(userModel.VkGroupId) && userModel.VkGroupId != vkInfo.GroupId)
                {
                    vkInfo.GroupId = userModel.VkGroupId;
                    await externalServiceRepo.UpdateAsync(vkInfo);
                }
            }

            if(user.Id != currentUserId)
            {
                userModel.IsFollowing = await followersRepo.IsFollowerAsync(currentUserId, user.Id);
                userModel.IsFollower = await followersRepo.IsFollowerAsync(user.Id, currentUserId);
                userModel.CanWrite = !(await blackListRepo.IsInBlackListAsync(currentUserId, user.Id));
                userModel.BlackListed = await blackListRepo.IsInBlackListAsync(user.Id, currentUserId);
            }
            return userModel;
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

        public async Task<DataServiceResult> UpdateUserAsync(NotifySettingsModel model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            user.MailNotify = model.MailNotify;
            try
            {
                await userManager.UpdateAsync(user);
                return Success;
            }
            catch(Exception e)
            {
                return CommonError("Внутренняя ошибка", e);
            }
        }

        public LandingModel GetLandingModel()
        {
            var usersRepo = Database.GetRepo<UserRepository, User>();
            return new LandingModel
            {
                AuthorsCount = usersRepo.Count(RoleNames.Author),
                ClientsCount = usersRepo.Count(RoleNames.Client),
                PostsCount = Database.GetRepo<PostsRepository, Post>().GetCount(),
                ServicesCount = Database.GetRepo<AuthorServiceRepository, AuthorService>().GetCount(),
                Users = Mapper.Map<IEnumerable<UserShortModel>>(usersRepo.RandomUsers(8))
            };
        }

        public async Task<string> GenerateInviteAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            user.InviteId = Guid.NewGuid();
            await userManager.UpdateAsync(user);
            return user.InviteId.ToString();
        }

        public async Task<IEnumerable<UserShortModel>> GetDayBestAsync()
        {
            var usersRepo = Database.GetRepo<UserRepository, User>();

            var now = DateTime.UtcNow;
            var beginDate = new DateTime(now.Year, now.Month, now.Day);
            var endDate = beginDate.AddHours(23).AddMinutes(59);

            const int count = 5;
            var users = await usersRepo.GetDateBestAsync(beginDate, endDate, count);
            return Mapper.Map<IEnumerable<UserShortModel>>(users);
        }
    }
}