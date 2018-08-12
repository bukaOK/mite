using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace Mite.Infrastructure
{
    public class TariffPaymentTimers
    {
        /// <summary>
        /// Словарь 
        /// </summary>
        public static Dictionary<Guid, Timer> TariffTimers { get; set; }
    }
}