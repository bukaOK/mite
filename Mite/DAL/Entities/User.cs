using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Mite.BLL.IdentityManagers;
using Mite.Constants;

namespace Mite.DAL.Entities
{
    
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        public byte? Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegisterDate { get; set; }
        /// <summary>
        /// Описание пользователя (блок "О себе")
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Путь к аватарке
        /// </summary>
        public string AvatarSrc { get; set; }
        /// <summary>
        /// Пол
        /// </summary>
        public byte Gender { get; set; }
        /// <summary>
        /// Местоположение(город)
        /// </summary>
        public string Placement { get; set; }
        /// <summary>
        /// Часовой пояс
        /// </summary>
        public short TimeZone { get; set; }
        public List<Post> Posts { get; set; }
        public List<Tag> Tags { get; set; }
        public int Rating { get; set; }
        public Group Group { get; set; }
        /// <summary>
        /// Подписчики пользователя
        /// </summary>
        public List<Follower> Followers { get; set; }
        /// <summary>
        /// Те, на кого пользователь подписан
        /// </summary>
        public string FollowIds { get; set; }
        /// <summary>
        /// Комментарии
        /// </summary>
        public List<Comment> Comments { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(AppUserManager manager)
        {
            if(AvatarSrc == null)
                AvatarSrc = "/Content/images/doubt-ava.png";
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim(ClaimConstants.AvatarSrc, AvatarSrc));
            return userIdentity;
        }
    }
}