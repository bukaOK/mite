using Mite.ExternalServices.Google.Attributes;
using Mite.ExternalServices.Google.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.ExternalServices.Google.AdSense
{
    public class AdSenseReportsRequestParams : RequestParams
    {
        public AdSenseReportsRequestParams(DateTime fromDate, DateTime toDate)
        {
            StartDate = fromDate.ToString("yyyy-MM-dd");
            EndDate = toDate.ToString("yyyy-MM-dd");
        }
        [ParamName("accountId")]
        public string AccountId { get; set; }
        [ParamName("endDate")]
        public string EndDate { get; set; }
        [ParamName("startDate")]
        public string StartDate { get; set; }
        [ParamName("currency")]
        public string Currency { get; set; }
        [ParamName("dimension")]
        public string Dimension { get; set; }
        [ParamName("filter")]
        public string Filter { get; set; }
        [ParamName("metric")]
        public string Metric { get; set; }
        [ParamName("sort")]
        public string Sort { get; set; }
        [ParamName("startIndex")]
        public int? StartIndex { get; set; }
        [ParamName("useTimezoneReporting")]
        public bool? UseTimezoneReporting { get; set; }
    }
}