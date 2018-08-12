﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Extensions;
using Mite.Models;
using System.Collections.Generic;
using Mite.Attributes.Filters;
using System;
using NLog;
using AutoMapper;
using System.Web;
using System.Text.RegularExpressions;
using Mite.Helpers;

namespace Mite.Controllers
{
    [Authorize]
    [AjaxOnly("Index")]
    public class UserSettingsController : BaseController
    {
        private readonly IUserService userService;
        private readonly AppUserManager userManager;
        private readonly IAuthenticationManager authManager;
        private readonly ITagsService tagsService;
        private readonly IExternalLinksService linksService;
        private readonly ICountryService countryService;
        private readonly IVkService vkService;
        private readonly IExternalServices externalServices;
        private readonly ITariffService tariffService;
        private readonly ILogger logger;
        private readonly ICityService cityService;

        public UserSettingsController(IUserService userService, AppUserManager userManager, ICityService cityService,
            IAuthenticationManager authManager, ITagsService tagsService, IExternalLinksService linksService, 
            ICountryService countryService, IVkService vkService, IExternalServices externalServices, 
            ITariffService tariffService, ILogger logger)
        {
            this.userService = userService;
            this.userManager = userManager;
            this.authManager = authManager;
            this.tagsService = tagsService;
            this.linksService = linksService;
            this.countryService = countryService;
            this.vkService = vkService;
            this.externalServices = externalServices;
            this.tariffService = tariffService;
            this.logger = logger;
            this.cityService = cityService;
        }
        public ViewResult Index()
        {
            return View();
        }
        public PartialViewResult ChangeAvatar()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<ActionResult> ChangeAvatar(HttpPostedFileBase img)
        {
            if (img == null)
                return Json(JsonStatuses.ValidationError, "Изображение не загружено");
            if (string.IsNullOrEmpty(img.ContentType) || img.ContentType.Split('/').FirstOrDefault() != "image")
                return Json(JsonStatuses.ValidationError, "Файл должен быть изображением");

            var imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

            var result = await userService.UpdateUserAvatarAsync(imagesFolder, img, CurrentUserId);
            var updatedUser = await userManager.FindByIdAsync(CurrentUserId);

            if (!result.Succeeded) return Json(JsonStatuses.ValidationError, "Неудача при сохранении");

            User.Identity.AddUpdateClaim(authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return Json(JsonStatuses.Success, "Аватарка обновлена");
        }
        public async Task<ActionResult> UserProfile()
        {
            var user = await userManager.FindByIdAsync(CurrentUserId);
            var model = Mapper.Map<ProfileSettingsModel>(user);
            model.Countries = await countryService.GetSelectListAsync(CurrentUserId);
            model.Cities = await cityService.GetSelectListAsync(CurrentUserId);

            model.Country = model.Countries.FirstOrDefault(x => x.Selected)?.Value;
            model.City = model.Cities.FirstOrDefault(x => x.Selected)?.Value;

            return PartialView(model);
        }
        public async Task<ActionResult> Invite()
        {
            var user = await userManager.FindByIdAsync(CurrentUserId);
            return PartialView(new InviteSettingsModel
            {
                InviteKey = User.IsInRole(RoleNames.Author) ? user.InviteId.ToString() : null
            });
        }
        [HttpPost]
        public async Task<ActionResult> ChangeClientRole(Guid inviteKey)
        {
            var user = await userManager.GetByInviteIdAsync(inviteKey);
            if(user != null)
            {
                var result = await userManager.AddToRoleAsync(CurrentUserId, RoleNames.Author);
                if (result.Succeeded)
                {
                    await userManager.RemoveFromRoleAsync(CurrentUserId, RoleNames.Client);
                    User.Identity.AddUpdateClaim(authManager, ClaimTypes.Role, RoleNames.Author);
                }
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<ActionResult> GenerateInvite()
        {
            var invite = await userService.GenerateInviteAsync(CurrentUserId);
            return Json(new { inviteId = invite });
        }
        public async Task<ActionResult> Notifications()
        {
            var logins = await userManager.GetLoginsAsync(CurrentUserId);
            var user = await userManager.FindByIdAsync(CurrentUserId);
            var model = new NotifySettingsModel
            {
                VkAuthenticated = logins.Any(x => x.LoginProvider == VkSettings.DefaultAuthType),
                MailNotify = user.MailNotify
            };
            return PartialView(model);
        }
        [HttpPost]
        public async Task<ActionResult> Notifications(NotifySettingsModel model)
        {
            var result = await userService.UpdateUserAsync(model, CurrentUserId);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        [HttpPost]
        public async Task<ActionResult> UserProfile(ProfileSettingsModel settings)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());

            var result = await userService.UpdateUserAsync(settings, User.Identity.GetUserId());
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.Error, result.Errors);
            }
            var updatedUser = await userManager.FindByIdAsync(User.Identity.GetUserId());

            User.Identity.AddUpdateClaim(authManager, ClaimTypes.Name, updatedUser.UserName);
            return Json(JsonStatuses.Success, "Сохранено");

        }
        public PartialViewResult Security()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePassModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, "Ошибка валидации");
            }
            var result = await userManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPass, model.NewPass);

            if (result.Succeeded)
            {
                return Json(JsonStatuses.Success);
            }
            return Json(JsonStatuses.ValidationError, "Ошибка при проверке пароля");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task ChangePhoneNum(string num)
        {
            var code = await userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), num);
            var msg = "Ваш код подтверждения: " + code;
            await userManager.SendSmsAsync(User.Identity.GetUserId(), msg);
        }
        public ActionResult EmailSettings()
        {
            var model = new EmailSettingsModel
            {
                Email = User.Identity.GetClaimValue(ClaimTypes.Email) ?? userManager.GetEmail(CurrentUserId),
                Confirmed = userManager.IsEmailConfirmed(CurrentUserId)
            };
            return PartialView(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task SendEmailConfirmation()
        {
            var userId = User.Identity.GetUserId();
            var code = await userManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Url.Scheme);

            await userManager.SendEmailAsync(userId, "MiteGroup.Подтверждение почты.", $"Для подтверждения вашего аккаунта перейдите по <a href=\"{callbackUrl}\">ссылке</a>. MiteGroup.");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeEmail(EmailSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            }
            var errors = new List<string>();
            var user = await userManager.FindByIdAsync(User.Identity.GetUserId());

            var emailUser = await userManager.FindByEmailAsync(model.NewEmail);
            if (emailUser != null && emailUser.Id != user.Id)
                errors.Add("Такой e-mail уже существует");

            var isPasswordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                errors.Add("Неверный пароль");

            //var isEmailConfimed = await userManager.IsEmailConfirmedAsync(user.Id);
            //if (isEmailConfimed)
            //    errors.Add("Ваш e-mail уже подтвержден");

            if (errors.Count > 0)
                return Json(JsonStatuses.ValidationError, errors);

            user.EmailConfirmed = false;
            user.Email = model.NewEmail;
            await userManager.UpdateAsync(user);
            return Json(JsonStatuses.Success, "E-mail успешно обновлен");
        }
        public async Task<ActionResult> UserTags()
        {
            var tags = await tagsService.GetForUserTagsAsync(CurrentUserId);
            return PartialView(tags);
        }
        public async Task<ActionResult> ClientTariffs()
        {
            var tariffs = await tariffService.GetForClientAsync(CurrentUserId);
            return PartialView(tariffs);
        }
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ActionResult> AuthorTariffs()
        {
            var tariffs = await tariffService.GetForAuthorAsync(CurrentUserId);
            return PartialView(tariffs);
        }
        public async Task<ActionResult> ShowOnlyFollowings(bool show)
        {
            try
            {
                var user = await userManager.FindByIdAsync(CurrentUserId);
                user.ShowOnlyFollowings = show;
                await userManager.UpdateAsync(user);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка в ShowOnlyFollowings: " + e.Message);
                return InternalServerError();
            }
        }
        public async Task<ActionResult> ExternalLinks()
        {
            var links = await linksService.GetByUserAsync(CurrentUserId);
            return PartialView(links);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateLinks(IEnumerable<ExternalLinkModel> models)
        {
            foreach(var item in models)
            {
                if (string.IsNullOrEmpty(item.Url))
                    ModelState.AddModelError("ExternalLink[]", "Пустая ссылка");
                if(!Regex.IsMatch(item.Url, @"^https?:\/\/[a-zA-Z\.\/]+"))
                    ModelState.AddModelError("ExternalLink[]", "Неверный формат ссылки");
            }
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            var result = await linksService.AddAsync(models, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            else
                return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> ParseVkId(string vkDomain)
        {
            if (Regex.IsMatch(vkDomain, "https?://"))
                vkDomain = Regex.Replace(vkDomain, "https?://vk.com/", string.Empty);
            var result = await vkService.GetGroupIdAsync(CurrentUserId, vkDomain);
            return result.Succeeded ? Json(JsonStatuses.Success, new
            {
                id = result.ResultData
            }) : Json(JsonStatuses.ValidationError, new
            {
                message = result.Errors.First()
            });
        }
    }
}