using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class SocialService
    {
        [Key]
        [MaxLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// Имя которое отображается для пользователя
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; }
        /// <summary>
        /// Что-то вроде https://vk.com/
        /// Чтобы чуваку осталось записать только свой ник
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ServiceBaseLink { get; set; }
        /// <summary>
        /// Путь к изображению(тут либо изображение либо иконка)
        /// </summary>
        public string ImgSrc { get; set; }
        /// <summary>
        /// Имя иконки в semantic
        /// </summary>
        public string IconName { get; set; }
        public IEnumerable<UserSocialService> UserSocialServices { get; set; }
    }
}