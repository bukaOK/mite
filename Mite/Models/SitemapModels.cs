using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Mite.Models
{
    [XmlRoot("urlset", Namespace = "https://www.sitemaps.org/schemas/sitemap/0.9")]
    public class SitemapUrlSet
    {
        [XmlElement("url")]
        public List<SitemapNode> UrlList { get; set; }
    }
    public class SitemapNode
    {
        [XmlElement("changefreq")]
        public string Frequency { get; set; }
        [XmlElement("lastmod")]
        public DateTime? LastModified { get; set; }
        [XmlElement("priority")]
        public double? Priority { get; set; }
        [XmlElement("loc")]
        public string Url { get; set; }
    }
}