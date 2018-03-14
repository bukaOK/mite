using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.Models;
using Mite.Core;
using Mite.CodeData.Constants;
using Mite.Attributes.Filters;
using Mite.ExternalServices.Google;
using System.Security.Claims;
using Mite.CodeData;

namespace Mite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService userService;
        private readonly AppUserManager userManager;
        private readonly IAuthenticationManager authManager;
        private readonly IExternalServices externalServices;
        private readonly IGoogleAdSenseService googleService;

        public AccountController(IUserService userService, AppUserManager userManager, IAuthenticationManager authManager,
            IExternalServices externalServices, IGoogleAdSenseService googleService)
        {
            this.userService = userService;
            this.userManager = userManager;
            this.authManager = authManager;
            this.externalServices = externalServices;
            this.googleService = googleService;
        }
        [OnlyGuests]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginModel { Remember = true };
            return View(model);
        }
        
        [HttpPost]
        [OnlyGuests]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
#if !DEBUG
            var recaptchaResult = await googleService.RecaptchaValidateAsync(Request["g-recaptcha-response"]);
            if (!recaptchaResult)
            {
                ModelState.AddModelError("", "Ошибка ReCaptcha.");
            }
#endif
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await userService.LoginAsync(model);

            switch (result)
            {
                case SignInStatus.Success:
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    return RedirectToAction("Top", "Posts");
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
        /// <summary>
        /// Вызывается при нажатии сабмит кнопки на входе или регистрации
        /// </summary>
        /// <param name="provider">Facebook, Google etc.</param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InitExternalAuth(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalRegister", "Account", new { returnUrl }));
        }
        public async Task<ActionResult> ExternalRegister(string returnUrl)
        {
            var loginInfo = await authManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }
            if (User.Identity.IsAuthenticated)
            {
                await userManager.AddLoginAsync(CurrentUserId, loginInfo.Login);
            }
            // Вход, если у пользователя есть аккаунт
            var result = await userService.LoginAsync(loginInfo, true);
            switch (result)
            {
                case SignInStatus.Success:
                    //Обновляем записи о сервисе
                    var accessToken = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceToken);
                    //var expires = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceExpires);
                    await externalServices.UpdateAsync(loginInfo.Login.ProviderKey, loginInfo.Login.LoginProvider, accessToken);

                    if(!string.IsNullOrEmpty(returnUrl))
                        return RedirectToLocal(returnUrl);
                    return RedirectToAction("Top", "Posts");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Если аккаунта нет перенаправляем на форму для создания
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View(new ShortRegisterModel { Email = loginInfo.Email });
            }
        }
        /// <summary>
        /// Вызывается после всех аутентификаций в случае, если пользователь уже зарегистрирован на сайте
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult ExternalLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel { Remember = true });
        }
        [HttpPost]
        [OnlyGuests]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLogin(LoginModel model, string returnUrl)
        {
            //Сначала логиним пользователя как обычно
            var result = await userService.LoginAsync(model);
            switch (result)
            {
                //Если все ок, добавляем токен соц. сети в базу
                case SignInStatus.Success:
                    var loginInfo = await authManager.GetExternalLoginInfoAsync();
                    var user = await userManager.FindByNameAsync(model.UserName);

                    if(loginInfo != null)
                    {
                        await userManager.AddLoginAsync(user.Id, loginInfo.Login);

                        var accessToken = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceToken);
                        var expires = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceExpires);
                        await externalServices.Add(user.Id, loginInfo.Login.LoginProvider, accessToken);
                    }
                    if(!string.IsNullOrEmpty(returnUrl))
                        return RedirectToLocal(returnUrl);
                    return RedirectToAction("Top", "Posts");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    ViewBag.ReturnUrl = returnUrl;
                    AddErrors(new[] { "Неверная пара логин/пароль" });
                    return View(model);
            }
        }
        [OnlyGuests]
        public ActionResult Register(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [OnlyGuests]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model, string returnUrl)
        {
            if ((RegisterRoles?)model.RegisterRole == RegisterRoles.Author && string.IsNullOrEmpty(model.InviteKey))
                ModelState.AddModelError("InviteKey", "Введите код приглашения");
#if !DEBUG
            var recaptchaResult = await googleService.RecaptchaValidateAsync(Request["g-recaptcha-response"]);
            if (!recaptchaResult)
            {
                ModelState.AddModelError("", "Ошибка ReCaptcha.");
            }
#endif
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await userService.RegisterAsync(model);
            if (result.Succeeded)
            {
                //Отправляем e-mail для подтверждения адреса эл. почты
                var user = await userManager.FindByNameAsync(model.UserName);
                string code = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Url.Scheme);
#if !DEBUG
                await userManager.SendEmailAsync(user.Id, "MiteGroup.Подтверждение почты.", $"Для подтверждения вашего аккаунта перейдите по <a href=\"{callbackUrl}\">ссылке.</a> MiteGroup.");
#endif
                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Top", "Posts");
                else
                    return RedirectToLocal(returnUrl);
            }
            //Если что то пошло не так, отображаем форму заново(вместе с ошибками)
            AddErrors(result.Errors);
            return View(model);
        }
        [HttpPost]
        [OnlyGuests]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalRegister(ShortRegisterModel model, string returnUrl)
        {
            if ((RegisterRoles?)model.RegisterRole == RegisterRoles.Author && string.IsNullOrEmpty(model.InviteKey))
                ModelState.AddModelError("InviteKey", "Введите код приглашения");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var loginInfo = await authManager.GetExternalLoginInfoAsync();
            if(loginInfo == null)
            {
                AddErrors(new[] { "Неудачная аутентификация." });
                return View(model);
            }
            var regModel = new RegisterModel
            {
                UserName = model.UserName,
                Email = model.Email,
                InviteKey = model.InviteKey,
                RegisterRole = model.RegisterRole
            };
            var result = await userService.RegisterAsync(regModel, loginInfo);
            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                //Добавляем внешний сервис
                var accessToken = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceToken);
                var expires = loginInfo.ExternalIdentity.FindFirstValue(ClaimConstants.ExternalServiceExpires);
                await externalServices.Add(user.Id, loginInfo.Login.LoginProvider, accessToken);
                //Отправляем e-mail для подтверждения адреса эл. почты
                string code = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, Request.Url.Scheme);

                await userManager.SendEmailAsync(user.Id, "MiteGroup.Подтверждение почты.", $"Для подтверждения вашего аккаунта перейдите по <a href=\"{callbackUrl}\">ссылке.</a> MiteGroup.");
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Top", "Posts");
            }
            AddErrors(result.Errors);
            //Если что то пошло не так, отображаем форму заново(вместе с ошибками)
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
            if (string.IsNullOrEmpty(model.Email))
                ModelState.AddModelError("Email", "Email не может быть пустым");

            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());

            var user = await userManager.FindByEmailAsync(model.Email);
            if(user == null)
                return Json(JsonStatuses.ValidationError, "Пользователя с таким e-mail не существует");

            if (!userManager.IsEmailConfirmed(user.Id))
                return Json(JsonStatuses.ValidationError, "Ваш e-mail не подтвержден");

            var code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code }, Request.Url.Scheme);
            var msg = $"Для восстановления пароля перейдите по ссылке -> <a href=\"{callbackUrl}\">жмак</a>";
            await userManager.SendEmailAsync(user.Id, "Восстановление пароля", msg);
            return Json(JsonStatuses.Success, "На ваш почтовый ящик отправлено сообщение с ссылкой для восстановления. Если сообщения нет во входящих, проверьте папку \"Спам\".");
        }
        [Authorize]
        public ActionResult LogOff()
        {
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            var result = await userManager.ConfirmEmailAsync(userId, code);
            if (User.Identity.IsAuthenticated)
            {
                return Redirect($"{Request.Url.Scheme}://{Request.Url.Host}/user/settings/security");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        public ViewResult ResetPassword(string code, string email)
        {
            var user = userManager.FindByEmail(email);
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
            var result = await userManager.ResetPasswordAsync(model.UserId, model.Code, model.NewPass);
            if(result.Succeeded)
                return RedirectToAction("Login", "Account");
            AddErrors(GetModelErrors());
            return View(model);
        }
        public async Task ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user != null)
            {
                var token = userManager.GeneratePasswordResetTokenAsync(user.Id);
                var url = Url.Action("ResetPassword", "Account", new { code = token, userId = user.Id }, Request.Url.Scheme);
                await userManager.SendEmailAsync(user.Id, "MiteGroup.Сброс пароля", $"Для сброса пароля переходим <a href=\"{url}\">по ссылке</a>. " +
                    $"С уважением, MiteGroup.");
            }
        }
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}