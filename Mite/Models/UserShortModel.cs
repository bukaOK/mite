using System;

namespace Mite.Models
{
    public class UserShortModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string AvatarSrc { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}