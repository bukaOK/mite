using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.DTO
{
    public class TagDTO
    {
        public string Name { get; set; }
        public int? Popularity { get; set; }
        public PostDTO Post { get; set; }
    }
}