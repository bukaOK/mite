using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace Mite.Core
{
    public abstract class BaseController : Controller
    {
        protected JsonResult JsonResponse(JsonResponseStatuses status)
        {
            return Json(new { status = status });
        }
        protected JsonResult JsonResponse(JsonResponseStatuses status, string message, object data)
        {
            
            return Json(JsonConvert.SerializeObject(new
            {
                status = status,
                message = message,
                data = data
            }));
        }

        protected JsonResult JsonResponse(JsonResponseStatuses status, object data)
        {
            return Json(JsonConvert.SerializeObject(new
            {
                status = status,
                data = data
            }));
        }

        protected JsonResult JsonResponse(JsonResponseStatuses status, string message)
        {
            if (status == JsonResponseStatuses.Error)
                Response.StatusCode = 500;
            return Json(new
            {
                status = status,
                message = message
            });
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
    public enum JsonResponseStatuses
    {
        Success, Error, ValidationError
    }
}