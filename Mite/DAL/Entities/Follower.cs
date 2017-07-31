using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Follower : GuidEntity
    {
        /// <summary>
        /// Id пользователя на кого подписан
        /// </summary>
        [ForeignKey("FollowingUser")]
        public string FollowingUserId { get; set; }
        /// <summary>
        /// Когда пользователь подписался
        /// </summary>
        public DateTime FollowTime { get; set; }
        /// <summary>
        /// Кто подписался
        /// </summary>
        [ForeignKey("User"), Required]
        public string UserId { get; set; }
        public User User { get; set; }
        public User FollowingUser { get; set; }
    }
}