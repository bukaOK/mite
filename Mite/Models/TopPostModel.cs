using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class TopPostModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsImage { get; set; }
        public DateTime LastEdit { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public int Views { get; set; }
        public List<string> Tags { get; set; }
        public UserShortModel User { get; set; }
    }
}