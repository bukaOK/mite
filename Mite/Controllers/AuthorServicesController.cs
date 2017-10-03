using Mite.BLL.Services;
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
        public async Task<ActionResult> Show(Guid id)
        {
            var authorService = await authorServiceService.GetShowAsync(id);
            if (authorService == null)
                return NotFound();
            return View(authorService);
        }
    }
}