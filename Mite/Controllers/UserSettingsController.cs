﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Constants;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.Extensions;
using Mite.Models;

namespace Mite.Controllers
{
    [Authorize]
    public class UserSettingsController : BaseController
    {
        private readonly IUserService _userService;
        private readonly AppUserManager _userManager;
        private readonly IAuthenticationManager _authManager;

        public UserSettingsController(IUserService userService, AppUserManager userManager,
            IAuthenticationManager authManager)
        {
            _userService = userService;
            _userManager = userManager;
            _authManager = authManager;
        }
        public ViewResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ChangeAvatar()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<JsonResult> ChangeAvatar(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str))
            {
                return JsonResponse(JsonResponseStatuses.Error, "Изображение не загружено");
            }
            var imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

            var result = await _userService.UpdateUserAvatarAsync(imagesFolder, base64Str, User.Identity.GetUserId());
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            if (!result.Succeeded) return JsonResponse(JsonResponseStatuses.Error, "Неудача при сохранении");

            User.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return JsonResponse(JsonResponseStatuses.Success, "Аватарка обновлена");
        }
        public ActionResult UserProfile()
        {
            var user = _userManager.FindById(User.Identity.GetUserId());
            var model = new ProfileSettingsModel
            {
                NickName = user.UserName,
                Gender = user.Gender,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                About = user.Description
            };

            return PartialView(model);
        }

        [HttpPost]
        public async Task<JsonResult> UserProfile(ProfileSettingsModel settings)
        {
            if (!ModelState.IsValid)
            {
                var errorList =
                    ModelState.Values.Select(modelState => modelState.Errors.Select(error => error.ErrorMessage))
                        .ToList();
                return JsonResponse(JsonResponseStatuses.ValidationError, errorList);
            }
            var result = await _userService.UpdateUserAsync(settings, User.Identity.GetUserId());
            if (!result.Succeeded) return JsonResponse(JsonResponseStatuses.Error, "Ошибка при сохранении");
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            User.AddUpdateClaim(_authManager, ClaimTypes.Name, updatedUser.UserName);
            User.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return JsonResponse(JsonResponseStatuses.Success, "Сохранено");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangePassword(ChangePassModel model)
        {
            if (!ModelState.IsValid)
            {
                return JsonResponse(JsonResponseStatuses.ValidationError, "Ошибка валидации");
            }
            var result = await _userManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPass, model.NewPass);

            if (result.Succeeded)
            {
                return JsonResponse(JsonResponseStatuses.Success);
            }
            return JsonResponse(JsonResponseStatuses.ValidationError, "Ошибка при проверке пароля");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task ChangePhoneNum(string num)
        {
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), num);
            var msg = "Ваш код подтверждения: " + code;
            await _userManager.SendSmsAsync(User.Identity.GetUserId(), msg);
        }
    }
}