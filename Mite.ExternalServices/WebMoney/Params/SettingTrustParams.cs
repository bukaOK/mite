using Mite.ExternalServices.WebMoney.Business;
using Mite.ExternalServices.WebMoney.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Params
{
    public class SettingTrustParams
    {
        public Phone ClientPhone { get; set; }
        public ExpressConfirmTypes ConfirmType { get; set; }
        public ExpressClientLoginTypes LoginType { get; set; }
        /// <summary>
        /// Максимальный платеж в день
        /// </summary>
        public double DayLimit { get; set; }
    }
}
