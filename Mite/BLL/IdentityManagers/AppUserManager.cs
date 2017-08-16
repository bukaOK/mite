using System;
using System.Web.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Mite.DAL.Entities;
using Mite.CodeData.Enums;
using Mite.DAL.Infrastructure;
using System.Security.Claims;
using System.Threading.Tasks;
using Mite.CodeData.Constants;

namespace Mite.BLL.IdentityManagers
{
    public class AppUserManager : UserManager<User>
    {
        public AppUserManager(IUserStore<User> store, IDataProtectionProvider dataProtectionProvider)
            : base(store)
        {
            UserValidator = new UserValidator<User>(this)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            MaxFailedAccessAttemptsBeforeLockout = 5;

            RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<User>
            {
                MessageFormat = "Your security code is {0}"
            });
            RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<User>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            EmailService = new EmailService();
            SmsService = new SmsService();
            if (dataProtectionProvider != null)
            {
                UserTokenProvider =
                    new DataProtectorTokenProvider<User>(dataProtectionProvider.Create("{861FD88D-77F1-4BE8-8660-5A6E283511D0}"));
            }
        }
        public string GetDefaultAvatarSrc()
        {
            var path = HostingEnvironment.ApplicationVirtualPath;
            if (path[path.Length - 1] != '\\')
                path += "\\";
            path += "default-ava.png";
            return path;
        }
        public async override Task<ClaimsIdentity> CreateIdentityAsync(User user, string authenticationType)
        {
            if (user.AvatarSrc == null)
                user.AvatarSrc = "/Content/images/doubt-ava.png";
            var userIdentity = await base.CreateIdentityAsync(user, authenticationType);
            userIdentity.AddClaim(new Claim(ClaimConstants.AvatarSrc, user.AvatarSrc));
            return userIdentity;
        }
    }
}