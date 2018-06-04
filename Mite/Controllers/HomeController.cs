using Hangfire;
using Mite.BLL.Services;
using Mite.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService userService;
        private readonly IExternalLinksService linksService;

        public HomeController(IUserService userService, IExternalLinksService linksService)
        {
            this.userService = userService;
            this.linksService = linksService;
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Top", "Posts");
            var model = userService.GetLandingModel();
            return View("Land", model);
        }
        [Route("away")]
        public async Task<ActionResult> Away(string url)
        {
            var isConfirmed = await linksService.IsConfirmedAsync(url);
            if (isConfirmed)
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
    }
}