using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class PostGroup : Entity
    {
        public string Name { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}