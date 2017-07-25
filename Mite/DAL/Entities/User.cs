using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Mite.BLL.IdentityManagers;
using Mite.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
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
        /// Показывать ли рекламу(только для авторов)
        /// </summary>
        public bool ShowAd { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<Tag> Tags { get; set; }
        /// <summary>
        /// Рейтинг(только для авторов)
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Подписчики пользователя
        /// </summary>
        public IList<Follower> Followers { get; set; }
        /// <summary>
        /// Комментарии
        /// </summary>
        public IList<Comment> Comments { get; set; }
        /// <summary>
        /// Номер Яндекс кошелька
        /// </summary>
        public string YandexWalId { get; set; }
        /// <summary>
        /// Id пользователя, который пригласил данного
        /// </summary>
        [ForeignKey("Referer")]
        public string RefererId { get; set; }
        public User Referer { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(AppUserManager manager)
        {
            if(AvatarSrc == null)
                AvatarSrc = "/Content/images/doubt-ava.png";
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            userIdentity.AddClaim(new Claim(ClaimConstants.AvatarSrc, AvatarSrc));
            return userIdentity;
        }
    }
}