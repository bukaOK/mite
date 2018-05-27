using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class ProductsController : BaseController
    {
        private readonly IProductsService productsService;

        public ProductsController(IProductsService productsService)
        {
            this.productsService = productsService;
        }
        [ChildActionOnly]
        public ActionResult Edit(Guid? id)
        {
            var product = id != null ? productsService.GetForEdit((Guid)id) : new ProductModel();
            return PartialView(product);
        }
        public async Task<ActionResult> Buy(Guid id)
        {
            var result = await productsService.BuyAsync(id, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Top()
        {
            var userId = User.Identity.IsAuthenticated ? CurrentUserId : null;
            var model = await productsService.GetTopModelAsync();

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Top(ProductTopFilterModel filterModel)
        {
            var result = await productsService.GetTopAsync(filterModel);
            return Json(JsonStatuses.Success, result);
        }
        public async Task<ActionResult> Download(Guid id)
        {
            var result = await productsService.DownloadPurchaseAsync(id, CurrentUserId);
            if (result.Succeeded)
            {
                return File(result.ResultData as MemoryStream, "application/zip", id.ToString() + ".zip");
            }
            return BadRequest();
        }
    }
}