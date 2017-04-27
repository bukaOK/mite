using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class ErrorController : Controller
    {
        private const string ErrorTemplate = "Template";
        /// <summary>
        /// Ошибка 404
        /// </summary>
        /// <returns></returns>
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Title = "Страница не найдена";
            ViewBag.ErrorImg = "/Content/images/404err.png";
            return View(ErrorTemplate);
        }
        public ActionResult InternalError()
        {
            Response.StatusCode = 500;
            ViewBag.Title = "Внутренняя ошибка сервера";
            ViewBag.ErrorImg = "/Content/images/500err.png";
            return View(ErrorTemplate);
        }
    }
}