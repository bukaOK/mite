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
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginModel { Remember = true };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
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

        [HttpGet]
        public ActionResult Register()
        {
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
            if(emailUser != null)
            {
                ModelState.AddModelError("EmailNotUnique", "Пользователь с таким e-mail уже существует.");
            }
            var nameUser = await _userManager.FindByNameAsync(model.UserName);
            if(nameUser != null)
            {
                ModelState.AddModelError("UserNameNotUnique", "Пользователь с таким именем уже существует.");
            }
            var result = await _userService.RegisterAsync(model);
            if (result.Succeeded)
            {
                //Отправляем e-mail для подтверждения адреса эл. почты
                //var user = await _userManager.FindByNameAsync(model.UserName);
                //string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code });

                //await _userManager.SendEmailAsync(user.Id, "Подтверждение пароля", "Подтверждаем свой аккаунт. <a href=\"" + callbackUrl + "\">Жмак.</a>");
                return RedirectToAction("Index", "Home");
            }
            AddErrors(result.Errors);
            //Если что то пошло не так, отображаем форму заново(вместе с ошибками)
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        [HttpPost]
        public async Task<bool> IsEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
        [HttpPost]
        public async Task<bool> IsUserNameExist(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user != null;
        }
        public async Task<HttpStatusCodeResult> ConfirmEmail(string userId, string code)
        {
            var result = await _userManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        public ActionResult ResetPassword(string code, string userId)
        {
            return View(new ResetPasswordModel { Code = code, UserId = userId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpStatusCodeResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return Forbidden();
            }
            await _userManager.ResetPasswordAsync(model.UserId, model.Code, model.NewPass);
            return Ok();
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