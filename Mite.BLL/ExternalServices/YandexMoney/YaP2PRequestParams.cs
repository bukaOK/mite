using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Yandex.Money.Api.Sdk.Requests;
using Yandex.Money.Api.Sdk.Utils;

namespace Mite.BLL.ExternalServices.YandexMoney
{
    public class YaP2PRequestPaymentParams : P2PRequestPaymentParams
    {
        [ParamName("amount")]
        public string Amount { get; set; }
    }
}