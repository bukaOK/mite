using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = RoleNames.Author)]
    public class TariffController : BaseController
    {
        private readonly ITariffService tariffService;
        private readonly AppUserManager userManager;

        public TariffController(ITariffService tariffService, AppUserManager userManager)
        {
            this.tariffService = tariffService;
            this.userManager = userManager;
        }

        public ActionResult Add()
        {
            return View("Edit", new TariffModel());
        }
        public async Task<ActionResult> Edit(string id)
        {
            if(Guid.TryParse(id, out Guid gId))
            {
                var model = await tariffService.GetAsync(gId);
                return View(model);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult> Add(TariffModel model)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            model.AuthorId = CurrentUserId;
            var result = await tariffService.AddAsync(model);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Update(TariffModel model)
        {
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            model.AuthorId = CurrentUserId;
            var result = await tariffService.UpdateAsync(model);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [Authorize]
        [Route("tariff/list/{authorname}")]
        public async Task<ActionResult> ClientList(string authorName)
        {
            var model = await tariffService.GetForClientAsync(CurrentUserId, authorName);
            return View("TariffsForClients", model);
        }
        public async Task<ActionResult> Bill(Guid id)
        {
            var result = await tariffService.AddForClientAsync(id, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        public async Task<ActionResult> ClientRemove(Guid tariffId)
        {
            var result = await tariffService.RemoveForClientAsync(tariffId, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
    }
}