using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    [Obsolete("Заменено ExternalLinks")]
    public class SocialLinks
    {
        public string Vk { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Dribbble { get; set; }
        public string ArtStation { get; set; }
        public string Instagram { get; set; }
        [Key, ForeignKey("User"), Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}