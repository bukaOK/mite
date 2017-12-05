using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Filters
{
    public class ServiceTopFilter
    {
        public Guid? CityId { get; set; }
        public Guid? ServiceTypeId { get; set; }
        public string Input { get; set; }
        public ServiceSortFilter SortFilter { get; set; }
        public int? Min { get; set; }
        public int Range { get; set; }
        public int Offset { get; set; }
        public int? Max { get; set; }
        public DateTime MaxDate { get; set; }
    }
}
