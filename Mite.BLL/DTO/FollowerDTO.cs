using Mite.BLL.Core;
using System;

namespace Mite.BLL.DTO
{
    public class FollowerDTO: GuidDTO
    {
        /// <summary>
        /// Id пользователя на кого подписан
        /// </summary>
        public string FollowingUserId { get; set; }
        /// <summary>
        /// Когда пользователь подписался
        /// </summary>
        public DateTime FollowTime { get; set; }
        /// <summary>
        /// Кто подписался
        /// </summary>
        public string UserId { get; set; }
        public UserDTO User { get; set; }
        public UserDTO FollowingUser { get; set; }
    }
}