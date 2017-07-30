using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Mite.Core
{
    public abstract class BaseController : Controller
    {
        protected JsonResult Json(JsonStatuses status)
        {
            return Json(new { status = status }, "application/json");
        }
        protected JsonResult Json(JsonStatuses status, string message, object data, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return Json(JsonConvert.SerializeObject(new
            {
                status = status,
                message = message,
                data = data
            }), "application/json", behavior);
        }

        protected JsonResult Json(JsonStatuses status, object data, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            return Json(JsonConvert.SerializeObject(new
            {
                status = status,
                data = data
            }), "application/json", behavior);
        }

        protected JsonResult Json(JsonStatuses status, string message, JsonRequestBehavior behavior = JsonRequestBehavior.DenyGet)
        {
            if (status == JsonStatuses.Error)
                Response.StatusCode = 500;
            return Json(new
            {
                status = status,
                message = message
            }, "application/json", behavior);
        }
        protected IEnumerable<string> GetErrorsList()
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0);
            foreach (var error in errors)
            {
                foreach (var valueError in error.Value.Errors)
                {
                    yield return valueError.ErrorMessage;
                }
            }
        }
        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        protected HttpStatusCodeResult Ok()
        {
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        protected HttpStatusCodeResult Forbidden()
        {
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
        protected HttpStatusCodeResult InternalServerError()
        {
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
        protected HttpStatusCodeResult BadRequest()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        protected HttpStatusCodeResult NotFound()
        {
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }
        protected IEnumerable<string> GetModelErrors()
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0);
            foreach (var error in errors)
            {
                foreach (var valueError in error.Value.Errors)
                {
                    yield return valueError.ErrorMessage;
                }
            }
        }
    }
    public enum JsonStatuses
    {
        Success, Error, ValidationError
    }
}