using Hangfire;
using Mite.BLL.Services;
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
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Top", "Posts");
            var model = userService.GetLandingModel();
            return View("Land", model);
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
        [Authorize(Roles = "admin")]
        public ActionResult RunHangfire()
        {
            BackgroundJob.Enqueue(() => HangfireConfig.LoadAdSenseIncome());
            return RedirectToAction("Index");
        }
    }
}