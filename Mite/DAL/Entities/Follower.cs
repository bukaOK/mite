using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Follower
    {
        /// <summary>
        /// Id подписчика
        /// </summary>
        [Key, ForeignKey("User")]
        public string Id { get; set; }
        /// <summary>
        /// Id пользователя на кого подписан
        /// </summary>
        [ForeignKey("FollowingUser")]
        [Required]
        public string FollowingUserId { get; set; }
        /// <summary>
        /// Когда пользователь подписался
        /// </summary>
        public DateTime FollowTime { get; set; }
        [ForeignKey("Id")]
        public User User { get; set; }
        public User FollowingUser { get; set; }
    }
}