using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class SocialService
    {
        public string Name { get; set; }
        public string ServiceName { get; set; }
        public string ServiceBaseLink { get; set; }
        public string UserName { get; set; }
    }
}