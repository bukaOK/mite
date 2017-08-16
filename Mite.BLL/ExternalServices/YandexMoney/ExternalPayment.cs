using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.ExternalServices.YandexMoney
{
    /// <summary>
    /// Предназначен для хранения в сессии, пока осуществляется платеж
    /// </summary>
    public class ExternalPayment
    {
        public string RequestID { get; set; }
        public string InstanceID { get; set; }
        public double Sum { get; set; }
    }
}