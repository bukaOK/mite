using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Filters
{
    public class OrderTopFilter
    {
        public string Input { get; set; }
        public Guid? OrderTypeId { get; set; }
        public Guid? CityId { get; set; }
        public int Range { get; set; }
        public int Offset { get; set; }
        public DateTime MaxDate { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
        public OrderStatuses OpenStatus => OrderStatuses.Open;
    }
}
