using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    public class SocialLinksDTO
    {
        public string Vk { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Dribbble { get; set; }
        public string ArtStation { get; set; }
        public string Instagram { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}
