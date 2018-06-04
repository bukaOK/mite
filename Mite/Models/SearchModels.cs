using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class SearchModel
    {
        [JsonProperty("results")]
        public IEnumerable<SearchCategoryModel> Categories { get; set; }
    }
    public class SearchCategoryModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("results")]
        public IEnumerable<SearchItemModel> Items { get; set; }
    }
    public class SearchItemModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("image")]
        public string ImageSrc { get; set; }
        [JsonProperty("price")]
        public double? Price { get; set; }
    }
}