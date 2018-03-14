using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class LandingModel
    {
        public int AuthorsCount { get; set; }
        public int ClientsCount { get; set; }
        public int ServicesCount { get; set; }
        public int DealsCount { get; set; }
        public int PostsCount { get; set; }
        public IEnumerable<UserShortModel> Users { get; set; }
    }
}