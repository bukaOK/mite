using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Helpers
{
    public static class CookieHelper
    {
        private const string CookieName = "MiteCustomCookie";

        private static HttpCookie Init()
        {
            var cookie = new HttpCookie(CookieName);
            cookie.Expires = DateTime.Now.AddDays(3);
            return cookie;
        }
        public static string Get(string name)
        {
            var context = HttpContext.Current;
            var cookie = context.Request.Cookies[CookieName];
            if (cookie == null)
            {
                return null;
            }
            return cookie[name];
        }
        public static void Add(string name, string value)
        {
            var context = HttpContext.Current;
            var cookie = context.Request.Cookies[CookieName];
            if (cookie == null)
            {
                cookie = Init();
            }
            cookie[name] = value;
            context.Response.Cookies.Add(cookie);
        }
        public static void Clear()
        {
            var cookie = new HttpCookie(CookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}