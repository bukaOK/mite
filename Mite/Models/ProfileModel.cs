using System;
using System.Collections.Generic;

namespace Mite.Models
{
    public class ProfileModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarSrc { get; set; }
        public int Rating { get; set; }
        public int FollowersCount { get; set; }
        /// <summary>
        /// Блок "О себе"
        /// </summary>
        public string About { get; set; }
        /// <summary>
        /// Является ли текущий пользователь подписчиком
        /// пользователя страницы
        /// </summary>
        public bool IsFollowing { get; set; }
        public int PostsCount { get; set; }
    }
}