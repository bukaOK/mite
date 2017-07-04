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
        public string AccountId { get; set; }
        public string EndDate { get; set; }
        public string StartDate { get; set; }
        public string Currency { get; set; }
        public string Dimension { get; set; }
        public string Filter { get; set; }
        public string Metric { get; set; }
        public string Sort { get; set; }
        public int? StartIndex { get; set; }
        public bool? UseTimezoneReporting { get; set; }
    }
}