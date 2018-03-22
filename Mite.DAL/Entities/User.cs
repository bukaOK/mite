using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.CodeData.Enums;
using System.ComponentModel;

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
        public City City { get; set; }
        [ForeignKey("City")]
        public Guid? CityId { get; set; }
        /// <summary>
        /// Показывать ли рекламу(только для авторов)
        /// </summary>
        public bool ShowAd { get; set; }
        /// <summary>
        /// Показывать только подписки
        /// </summary>
        public bool ShowOnlyFollowings { get; set; }
        /// <summary>
        /// Рейтинг(только для авторов)
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Теги, на которые подписался пользователь
        /// </summary>
        public IList<Tag> Tags { get; set; }
        /// <summary>
        /// Работы пользователя(только для авторов)
        /// </summary>
        public IList<Post> Posts { get; set; }

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
        /// <summary>
        /// Надежность(зависит от сделок)
        /// </summary>
        public int Reliability { get; set; }
        /// <summary>
        /// Уведомлять ли по почте
        /// </summary>
        public bool MailNotify { get; set; }
        /// <summary>
        /// Id для приглашения других участников
        /// </summary>
        public Guid? InviteId { get; set; }
        /// <summary>
        /// Кто может писать пользователю
        /// </summary>
        public ChatPrivacy ChatPrivacy { get; set; }
    }
}