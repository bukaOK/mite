using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class AdminStatisticModel
    {
        public int PostsCount { get; set; }
        public int UsersCount { get; set; }
    }
    public class AdminUserReviewModel
    {
        public Guid Id { get; set; }
        public UserShortModel User { get; set; }
        public string Review { get; set; }
    }
}