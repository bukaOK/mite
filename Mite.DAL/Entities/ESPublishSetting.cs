using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Настройки публикации на внешних сервисах(Вк, Facebook и т.д.)
    /// </summary>
    public class ESPublishSetting : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        public string VkGroupId { get; set; }
        /// <summary>
        /// Id группы(публичной страницы) которой владеет пользователь
        /// </summary>
        public string FacebookPageId { get; set; }
        /// <summary>
        /// Публиковать ли Вконтакте
        /// </summary>
        public bool PublishVk { get; set; }
        /// <summary>
        /// Публиковать ли на Facebook
        /// </summary>
        public bool PublishFb { get; set; }
        /// <summary>
        /// Публиковать ли на DeviantArt
        /// </summary>
        public bool PublishDeviant { get; set; }
        /// <summary>
        /// Публиковать ли в Твиттере
        /// </summary>
        public bool PublishOnTwitter { get; set; }
    }
}
