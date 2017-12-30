using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace Mite.Core
{
    public abstract class BaseController : Controller
    {
        protected string CurrentUserId => User.Identity.GetUserId();
        protected ContentResult Json(JsonStatuses status)
        {
            return Content(JsonConvert.SerializeObject(new { status }), "application/json");
        }
        protected ContentResult Json(JsonStatuses status, string message, object data)
        {
            return Content(JsonConvert.SerializeObject(new { status, message, data }), "application/json");
        }

        protected ContentResult Json(JsonStatuses status, object data)
        {
            return Content(JsonConvert.SerializeObject(new { status, data }), "application/json");
        }

        protected ContentResult Json(JsonStatuses status, string message)
        {
            if (status == JsonStatuses.Error)
                Response.StatusCode = 500;
            return Content(JsonConvert.SerializeObject(new { status, message }), "application/json");
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
        protected HttpStatusCodeResult InternalServerError(string message)
        {
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, message);
        }
        protected HttpStatusCodeResult InternalServerError(object obj)
        {
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(obj));
        }
        protected HttpStatusCodeResult BadRequest()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        protected HttpStatusCodeResult BadRequest(string message)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
        }
        protected HttpStatusCodeResult BadRequest(object obj)
        {
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(obj));
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