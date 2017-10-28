using System;

namespace Mite.Models
{
    public class UserShortModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        private string avatarSrc;
        public string AvatarSrc
        {
            get
            {
                if (avatarSrc == null)
                    return null;
                return avatarSrc.Replace('\\', '/');
            }
            set
            {
                avatarSrc = value;
            }
        }
        public string Description { get; set; }
        public int Rating { get; set; }
        public DateTime RegisterDate { get; set; }
        public int Reliability { get; set; }
    }
}