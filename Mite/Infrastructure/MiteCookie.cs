using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure
{
    public class MiteCookie
    {
        private const string CookieName = "MiteCustomCookie";
        public DateTime Expires
        {
            get
            {
                return Cookie.Expires;
            }
            set
            {
                Cookie.Expires = value;
            }
        }
        private HttpCookie Cookie { get; set; }
        private readonly HttpContext httpContext;

        public MiteCookie(HttpContext context)
        {
            httpContext = context;
            Cookie = context.Request.Cookies[CookieName];
            if (Cookie == null)
            {
                Cookie = new HttpCookie(CookieName);
            }
        }
        public string this[string name]
        {
            set
            {
                Cookie[name] = value;
            }
            get
            {
                return Cookie[name];
            }
        }
        /// <summary>
        /// Добавляем Cookie в ответ
        /// </summary>
        public void Save()
        {
            httpContext.Response.Cookies.Add(Cookie);
        }
        public void Clear()
        {
            var cookie = new HttpCookie(CookieName)
            {
                Expires = DateTime.Now.AddDays(-1)
            };
            httpContext.Response.Cookies.Add(Cookie);
        }
    }
}