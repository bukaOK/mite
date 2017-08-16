using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    class HelperDTO
    {
        public bool EditDocBtn { get; set; }
        public bool PublicPostsBtn { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}
