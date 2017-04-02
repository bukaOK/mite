using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Ошибка 404
        /// </summary>
        /// <returns></returns>
        public ActionResult NotFound()
        {
            ViewBag.HttpReferrer = Request.UrlReferrer;
            return View();
        }
    }
}