using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class UserSocialService
    {
        /// <summary>
        /// Какое значение нужно добавить к социальному сервису
        /// </summary>
        public string UserValue { get; set; }
        /// <summary>
        /// Имя соц. сервиса
        /// </summary>
        [Key, ForeignKey("SocialService"), Column(Order = 0)]
        public string SocialServiceName { get; set; }
        /// <summary>
        /// Id пользователя
        /// </summary>
        [Key, ForeignKey("User"), Column(Order = 1)]
        public string UserId { get; set; }
        public SocialService SocialService { get; set; }
        public User User { get; set; }
    }
}