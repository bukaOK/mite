using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class SitemapNode
    {
        public string Frequency { get; set; }
        public DateTime? LastModified { get; set; }
        public double? Priority { get; set; }
        public string Url { get; set; }
    }
}