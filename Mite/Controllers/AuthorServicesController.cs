﻿using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class AuthorServicesController : BaseController
    {
        private readonly IAuthorServiceService authorServiceService;

        public AuthorServicesController(IAuthorServiceService authorServiceService)
        {
            this.authorServiceService = authorServiceService;
        }
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ViewResult> Add()
        {
            ViewBag.Title = "Добавление услуги";

            var model = await authorServiceService.GetNew();
            return View("Edit", model);
        }
        [Authorize(Roles = RoleNames.Author)]
        public async Task<ActionResult> Edit(Guid id)
        {
            ViewBag.Title = "Изменение услуги";
            var authorService = await authorServiceService.GetAsync(id);
            if (authorService == null)
                return NotFound();
            return View(authorService);
        }
        public async Task<ActionResult> Show(Guid id)
        {
            var authorService = await authorServiceService.GetShowAsync(id);
            if (authorService == null)
                return NotFound();
            return View(authorService);
        }
        public async Task<ActionResult> Top()
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;
            var model = await authorServiceService.GetTopModelAsync(userId);

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Top(ServiceTopFilterModel filterModel)
        {
            var result = await authorServiceService.GetTopAsync(filterModel);
            return Json(JsonStatuses.Success, result);
        }
        public async Task<ActionResult> ServiceGallery(Guid id)
        {
            var result = await authorServiceService.GetGalleryAsync(id);
            return Json(JsonStatuses.Success, result);
        }
    }
}