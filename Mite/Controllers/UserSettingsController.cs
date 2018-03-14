using System.Linq;
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

namespace Mite.Controllers
{
    [Authorize]
    [AjaxOnly("Index")]
    public class UserSettingsController : BaseController
    {
        private readonly IUserService _userService;
        private readonly AppUserManager _userManager;
        private readonly IAuthenticationManager _authManager;
        private readonly ICityService cityService;

        public UserSettingsController(IUserService userService, AppUserManager userManager, ICityService cityService,
            IAuthenticationManager authManager)
        {
            _userService = userService;
            _userManager = userManager;
            _authManager = authManager;
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
        public async Task<ActionResult> ChangeAvatar(string base64Str)
        {
            if (string.IsNullOrEmpty(base64Str))
            {
                return Json(JsonStatuses.Error, "Изображение не загружено");
            }
            var imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";

            var result = await _userService.UpdateUserAvatarAsync(imagesFolder, base64Str, User.Identity.GetUserId());
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            if (!result.Succeeded) return Json(JsonStatuses.Error, "Неудача при сохранении");

            User.Identity.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
            return Json(JsonStatuses.Success, "Аватарка обновлена");
        }
        public ActionResult UserProfile()
        {
            var user = _userManager.FindById(CurrentUserId);
            var model = new ProfileSettingsModel
            {
                NickName = user.UserName,
                Gender = user.Gender,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                About = user.Description,
                Cities = cityService.GetCitiesSelectList(user),
            };

            return PartialView(model);
        }
        public ActionResult Invite()
        {
            var user = _userManager.FindById(CurrentUserId);
            return PartialView(new InviteSettingsModel
            {
                InviteKey = user.InviteId.ToString()
            });
        }
        [HttpPost]
        public async Task<ActionResult> GenerateInvite()
        {
            var invite = await _userService.GenerateInviteAsync(CurrentUserId);
            return Json(new { inviteId = invite });
        }
        public ActionResult Notifications()
        {
            var logins = _userManager.GetLogins(CurrentUserId);
            var user = _userManager.FindById(CurrentUserId);
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
            var result = await _userService.UpdateUserAsync(model, CurrentUserId);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        [HttpPost]
        public async Task<ActionResult> UserProfile(ProfileSettingsModel settings)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());

            var result = await _userService.UpdateUserAsync(settings, User.Identity.GetUserId());
            if (!result.Succeeded)
            {
                return Json(JsonStatuses.Error, result.Errors);
            }
            var updatedUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());

            User.Identity.AddUpdateClaim(_authManager, ClaimTypes.Name, updatedUser.UserName);
            User.Identity.AddUpdateClaim(_authManager, ClaimConstants.AvatarSrc, updatedUser.AvatarSrc);
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
        public ActionResult EmailSettings()
        {
            var userId = User.Identity.GetUserId();
            var model = new EmailSettingsModel
            {
                Email = User.Identity.GetClaimValue(ClaimTypes.Email) ?? _userManager.GetEmail(userId),
                Confirmed = _userManager.IsEmailConfirmed(userId)
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
        public async Task<ActionResult> ChangeEmail(EmailSettingsModel model)
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