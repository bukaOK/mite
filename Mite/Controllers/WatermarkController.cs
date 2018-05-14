using Mite.BLL.Helpers;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Models;
using System;
using System.Web.Mvc;
using System.Web.UI;

namespace Mite.Controllers
{
    public class WatermarkController : BaseController
    {
        private readonly IWatermarkService watermarkService;

        public WatermarkController(IWatermarkService watermarkService)
        {
            this.watermarkService = watermarkService;
        }
        [ChildActionOnly]
        public ActionResult Edit(Guid? id)
        {
            var wat = id != null ? watermarkService.Get((Guid)id) : new WatermarkEditModel();
            wat.FontSize = 15;
            wat.Gravity = ImageGravity.SouthEast;
            wat.WmText = $"mitegroup/{User.Identity.Name}";

            return PartialView(wat);
        }
        public ActionResult Draw(string wmText, int fontSize, bool inverted)
        {
            var bytes = ImagesHelper.DrawWatermark(fontSize, wmText, inverted);
            return File(bytes, "image/png");
        }
    }
}