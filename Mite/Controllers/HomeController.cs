using Hangfire;
using Mite.BLL.Services;
using Mite.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService userService;

        public HomeController(IUserService userService)
        {
            this.userService = userService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Top", "Posts");
            //if (User.Identity.IsAuthenticated)

            //var model = userService.GetLandingModel();
            //return View("Land", model);
        }
        [Route("away")]
        public ActionResult Away(string url)
        {
            var match = Regex.Match(url, System.IO.File.ReadAllText(Server.MapPath("/Files/awayConfirmed.txt")));
            if (match.Success && match.Groups[2].Success)
                return Redirect(url);
            else
                return View("Away", (object)url);
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
        public ActionResult Faq()
        {
            var model = JsonConvert.DeserializeObject<IEnumerable<FaqQuestionModel>>(System.IO.File.ReadAllText(Server.MapPath("/Files/faqData.json")));
            return View(model);
        }
        [Authorize(Roles = "admin")]
        public ActionResult RunHangfire()
        {
            BackgroundJob.Enqueue(() => HangfireConfig.LoadAdSenseIncome());
            return RedirectToAction("Index");
        }
        public ActionResult RunTariffsCheck()
        {
            BackgroundJob.Enqueue(() => HangfireConfig.TariffsCheckoutAsync());
            return RedirectToAction("Index");
        }
    }
}