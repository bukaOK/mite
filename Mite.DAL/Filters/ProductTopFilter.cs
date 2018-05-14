using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;

namespace Mite.DAL.Filters
{
    public class ProductTopFilter
    {
        public string Input { get; set; }
        public bool ForAuthors { get; set; }
        public Guid? CityId { get; set; }
        public int Offset { get; set; }
        public int Range { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public ProductFilter Sort { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime MinDate { get; set; }
        public List<Guid> PostIds { get; set; }
        public string[] Tags { get; set; }
    }
}
