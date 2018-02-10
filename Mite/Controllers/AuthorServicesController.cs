using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = RoleNames.Author)]
    public class AuthorServicesController : BaseController
    {
        private readonly IAuthorServiceService authorServiceService;

        public AuthorServicesController(IAuthorServiceService authorServiceService)
        {
            this.authorServiceService = authorServiceService;
        }
        public async Task<ViewResult> Add()
        {
            ViewBag.Title = "Добавление услуги";

            var model = await authorServiceService.GetNew();
            return View("Edit", model);
        }
        public async Task<ActionResult> Edit(Guid id)
        {
            ViewBag.Title = "Изменение услуги";
            var authorService = await authorServiceService.GetAsync(id);
            if (authorService == null)
                return NotFound();
            return View(authorService);
        }
        public async Task<ActionResult> AddVkServices()
        {
            var authorService = await authorServiceService.GetNew();
            return View("AddVkServices", Mapper.Map<VkServiceModel>(authorService));
        }
        [HttpPost]
        public async Task<ActionResult> AddVkServices(IList<VkServiceModel> services)
        {
            if (!ModelState.IsValid || services == null)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            var result = await authorServiceService.AddVkListAsync(services, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Show(Guid id)
        {
            var authorService = await authorServiceService.GetShowAsync(id);
            if (authorService == null)
                return NotFound();
            return View(authorService);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Top()
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;
            var model = await authorServiceService.GetTopModelAsync(userId);

            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Top(ServiceTopFilterModel filterModel)
        {
            var result = await authorServiceService.GetTopAsync(filterModel);
            return Json(JsonStatuses.Success, result);
        }
        [AllowAnonymous]
        public async Task<ActionResult> ServiceGallery(Guid id)
        {
            var result = await authorServiceService.GetGalleryAsync(id);
            return Json(JsonStatuses.Success, result);
        }
    }
}