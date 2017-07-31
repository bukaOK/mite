using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    [Obsolete("Заменено SocialServices, после обновления надо удалить")]
    public class SocialLinks : GuidEntity
    {
        public string Vk { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Dribbble { get; set; }
        public string ArtStation { get; set; }
        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}