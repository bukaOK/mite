using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
            var product = id != null ? productsService.GetForEdit((Guid)id) : new ProductEditModel();
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
        public async Task<ActionResult> ConfirmBuying(string code)
        {
            var result = a
        }
    }
}