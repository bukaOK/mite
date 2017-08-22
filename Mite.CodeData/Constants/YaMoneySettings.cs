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
        public static string ClientId
        {
            get
            {
                const string mitegroup = "87EAAB2289CB433AD2EA0B4DD6169271462C06F5E9EA8C8909E08E699AD501EA";
                const string localhost = "6F9233678143053F6301F508E2DD1261319C1F1E1652E6200087A408A213C0C4";
                const string mitetest = "6CA062C49493F5ABF15A5A0E6432DB66776182DB087D297F3E34A3908A266843";

                if (HttpContext.Current != null)
                {
                    var host = HttpContext.Current.Request.Url.Host;
                    switch (host)
                    {
                        case "mitegroup.ru":
                            return mitegroup;
                        case "test.mitegroup.ru":
                            return mitetest;
                        case "localhost":
                            return localhost;
                        default:
                            return mitegroup;
                    }
                }
                return mitegroup;
            }
        }
        public const string Secret = "7C92317DBA0BECF721C0BD7C17AA04A2195FCDB67FEE9E3411540248246B41D3F2455D5870C070BEAB3E35ECDD74AAF338392CEE17535BA4596E360C56E89D4B";
        public static string RedirectUri
        {
            get
            {
                var context = HttpContext.Current;
                if(context != null)
                {
                    string redirect;
                    if (context.Request.Url.IsDefaultPort)
                    {
                        redirect = $"http://{context.Request.Url.Host}/yandex/authorize";
                    }
                    else
                    {
                        redirect = $"http://{context.Request.Url.Host}:{context.Request.Url.Port}/yandex/authorize";
                    }
                    return redirect;
                }
                return "http://mitegroup.ru/yandex/authorize";
            }
        }
        public static string ExtPayRedirectSuccess
        {
            get
            {
                var context = HttpContext.Current;
                if(context != null)
                {
                    string redirect;
                    if (context.Request.Url.IsDefaultPort)
                    {
                        redirect = $"http://{context.Request.Url.Host}/yandex/confirmexternalpayment?success=true";
                    }
                    else
                    {
                        redirect = $"http://{context.Request.Url.Host}:{context.Request.Url.Port}/yandex/confirmexternalpayment?success=true";
                    }
                    return redirect;
                }
                return "http://mitegroup.ru/yandex/confirmexternalpayment?success=true";
            }   
        }
        public static string ExtPayRedirectFailed
        {
            get
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    string redirect;
                    if (context.Request.Url.IsDefaultPort)
                    {
                        redirect = $"http://{context.Request.Url.Host}/yandex/confirmexternalpayment?success=false";
                    }
                    else
                    {
                        redirect = $"http://{context.Request.Url.Host}:{context.Request.Url.Port}/yandex/confirmexternalpayment?success=false";
                    }
                    return redirect;
                }
                return "http://mitegroup.ru/yandex/confirmexternalpayment?success=false";
            }
        }
    }
}