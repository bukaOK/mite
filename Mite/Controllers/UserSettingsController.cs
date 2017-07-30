using System.Linq;
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
using Mite.Extensions;
using Mite.Models;
using System.Collections.Generic;
using Mite.Attributes.Filters;

namespace Mite.Controllers
{
    [Authorize]
    [AjaxOnly("Index")]
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
        public PartialViewResult ChangeAvatar()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<JsonResult> ChangeAvatar(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str))
            {
                return Json(JsonStatuses.Error, "Изображение не загружено");
            }
            var imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

            var result = await _userService.UpdateUserAvatarAsync(imagesFolder, base64Str, User.Identity.GetUserId());
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            if (!result.Succeeded) return Json(JsonStatuses.Error, "Неудача при сохранении");

            User.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return Json(JsonStatuses.Success, "Аватарка обновлена");
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
                return Json(JsonStatuses.ValidationError, errorList);
            }

            var result = await _userService.UpdateUserAsync(settings, User.Identity.GetUserId());
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.Error, result.Errors);
            }
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            User.AddUpdateClaim(_authManager, ClaimTypes.Name, updatedUser.UserName);
            User.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return Json(JsonStatuses.Success, "Сохранено");

        }
        public PartialViewResult Security()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangePassword(ChangePassModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, "Ошибка валидации");
            }
            var result = await _userManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPass, model.NewPass);

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
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), num);
            var msg = "Ваш код подтверждения: " + code;
            await _userManager.SendSmsAsync(User.Identity.GetUserId(), msg);
        }
        public async Task<PartialViewResult> EmailSettings()
        {
            var model = new EmailSettingsModel
            {
                Email = await _userManager.GetEmailAsync(User.Identity.GetUserId()),
                Confirmed = await _userManager.IsEmailConfirmedAsync(User.Identity.GetUserId())
            };
            return PartialView(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task SendEmailConfirmation()
        {
            var userId = User.Identity.GetUserId();
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userId, code = code }, "http");

            await _userManager.SendEmailAsync(userId, "MiteGroup.Подтверждение почты.", "Для подтверждения вашего аккаунта перейдите по <a href=\"" + callbackUrl + "\">ссылке.</a> MiteGroup.");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangeEmail(EmailSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            }
            var errors = new List<string>();
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            var emailUser = await _userManager.FindByEmailAsync(model.NewEmail);
            if (emailUser != null && emailUser.Id != user.Id)
                errors.Add("Такой e-mail уже существует");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                errors.Add("Неверный пароль");

            //var isEmailConfimed = await _userManager.IsEmailConfirmedAsync(user.Id);
            //if (isEmailConfimed)
            //    errors.Add("Ваш e-mail уже подтвержден");

            if (errors.Count > 0)
                return Json(JsonStatuses.ValidationError, errors);

            user.EmailConfirmed = false;
            user.Email = model.NewEmail;
            await _userManager.UpdateAsync(user);
            return Json(JsonStatuses.Success, "E-mail успешно обновлен");
        }
        public PartialViewResult SocialServices()
        {
            var links = _userService.GetSocialLinks(User.Identity.GetUserId());
            return PartialView(links);
        }
        [HttpPost]
        public async Task<HttpStatusCodeResult> UpdateSocialServices(SocialLinksModel model)
        {
            await _userService.UpdateSocialLinksAsync(model, User.Identity.GetUserId());
            return Ok();
        }
    }
}