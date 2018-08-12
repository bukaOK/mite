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
            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(404);
            ViewBag.Title = ViewBag.ErrorText = "404. Страница не найдена";
            return View(ErrorTemplate);
        }
        public ActionResult Forbidden()
        {
            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(403);
            ViewBag.Title = ViewBag.ErrorText = "403. Доступ запрещен";
            return View(ErrorTemplate);
        }
        public ActionResult InternalError()
        {
            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(500);
            ViewBag.Title = ViewBag.ErrorText = "500. Внутренняя ошибка сервера";
            return View(ErrorTemplate);
        }
    }
}