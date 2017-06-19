using Mite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Top", "Posts");
            }
            return View();
        }
        public ActionResult Help()
        {
            return View();
        }
        public ActionResult Donate()
        {
            return View();
        }
        public ActionResult Terms()
        {
            return View();
        }
        public ActionResult Privacy()
        {
            return View();
        }
        public ActionResult BadJs()
        {
            return View();
        }
    }
}