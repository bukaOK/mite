#define DEBUG

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Constants;
using Mite.Models;
using Mite.Core;
using Mite.Helpers;

namespace Mite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly AppUserManager _userManager;
        private readonly IAuthenticationManager _authManager;

        public AccountController(IUserService userService, AppUserManager userManager, IAuthenticationManager authManager)
        {
            _userService = userService;
            _userManager = userManager;
            _authManager = authManager;
        }
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "UserProfile");
            }
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginModel { Remember = true };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if(Request.Url.Host == "test.mitegroup.ru")
            {
                var allowedList = new List<string>
                {
                    "landenor",
                    "dindon"
                };
                if(!allowedList.Contains(model.UserName))
                {
                    ModelState.AddModelError("", "Вход запрещен.");
                    return View(model);
                }
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await _userService.LoginAsync(model);

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.Remember });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("SignInFail", "Неверная пара логин/пароль");
                    return View(model);
            }
        }
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "UserProfile");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var emailUser = await _userManager.FindByEmailAsync(model.Email);
            var nameUser = await _userManager.FindByNameAsync(model.UserName);
            if (emailUser != null || nameUser != null)
            {
                if(emailUser != null)
                    ModelState.AddModelError("EmailNotUnique", "Пользователь с таким e-mail уже существует.");
                if(nameUser != null)
                    ModelState.AddModelError("UserNameNotUnique", "Пользователь с таким именем уже существует.");
                return View(model);
            }
            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded)
            {
                //Отправляем e-mail для подтверждения адреса эл. почты
                var user = await _userManager.FindByNameAsync(model.UserName);
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, "http");

                await _userManager.SendEmailAsync(user.Id, "MiteGroup.Подтверждение почты.", "Для подтверждения вашего аккаунта перейдите по <a href=\"" + callbackUrl + "\">ссылке.</a> MiteGroup.");
                return RedirectToAction("Index", "Home");
            }
            //Если что то пошло не так, отображаем форму заново(вместе с ошибками)
            ModelState.AddModelError("", "Ошибка на сервере");
            Logger.WriteErrors("AccountController, Register", result.Errors);
            return View(model);
        }
        /// <summary>
        /// Восстановление пароля
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPassModel model)
        {
            if (!ModelState.IsValid)
                return JsonResponse(JsonResponseStatuses.ValidationError, "Неправильный e-mail");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
                return JsonResponse(JsonResponseStatuses.ValidationError, "Пользователя с таким e-mail не существует");

            if (!_userManager.IsEmailConfirmed(user.Id))
                return JsonResponse(JsonResponseStatuses.ValidationError, "Ваш e-mail не подтвержден");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code = code }, Request.Url.Scheme);
            var msg = $"Для восстановления пароля перейдите по ссылке -> <a href=\"{callbackUrl}\">жмак</a>";
            await _userManager.SendEmailAsync(user.Id, "Восстановление пароля", msg);
            return JsonResponse(JsonResponseStatuses.Success, "На ваш почтовый ящик отправлено сообщение с ссылкой для восстановления");
        }
        public ActionResult LogOff()
        {
            _authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            var result = await _userManager.ConfirmEmailAsync(userId, code);
            if (User.Identity.IsAuthenticated)
            {
                return Redirect($"http://{Request.Url.Host}/user/settings#/security");
            }
            else
            {
                Logger.WriteErrors("AccountController, ConfirmEmail", result.Errors);
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ResetPassword(string code, string email)
        {
            var user = _userManager.FindByEmail(email);
            return View(new ResetPasswordModel { Code = code, UserId = user.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            await _userManager.ResetPasswordAsync(model.UserId, model.Code, model.NewPass);
            return RedirectToAction("Login", "Account");
        }
        public async Task ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if(user != null)
            {
                var token = _userManager.GeneratePasswordResetTokenAsync(user.Id);
                var url = Url.Action("ResetPassword", "Account", new { code = token, userId = user.Id });
                await _userManager.SendEmailAsync(user.Id, "Сброс пароля", "Для сброса пароля переходим <a href=\""
                    + url + "\">по ссылке</a>");
            }
        }
        private const string XsrfKey = "{8DE6158A-638E-43A6-B77B-89819FFC9240}";

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}