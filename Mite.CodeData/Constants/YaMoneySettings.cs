using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Constants
{
    public static class YaMoneySettings
    {
        public const string WalletId = "410011411387333";
        public const string DefaultAuthType = "YandexMoney";
        public const string ClientId = "87EAAB2289CB433AD2EA0B4DD6169271462C06F5E9EA8C8909E08E699AD501EA";    
        public const string Secret = "7C92317DBA0BECF721C0BD7C17AA04A2195FCDB67FEE9E3411540248246B41D3F2455D5870C070BEAB3E35ECDD74AAF338392CEE17535BA4596E360C56E89D4B";
        public const string RedirectUri = "http://mitegroup.ru/yandex/authorize";
        public const string ExtPayRedirectSuccess = "http://mitegroup.ru/yandex/confirmexternalpayment?success=true";
        public const string ExtPayRedirectFailed = "http://mitegroup.ru/yandex/confirmexternalpayment?success=false";
    }
}