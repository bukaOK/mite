using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class CharacterController : BaseController
    {
        private readonly ICharacterService characterService;

        public CharacterController(ICharacterService characterService)
        {
            this.characterService = characterService;
        }
        public ActionResult Add()
        {
            return View("Edit", new CharacterModel());
        }
        public async Task<ActionResult> Edit(string id)
        {
            if(Guid.TryParse(id, out Guid gId))
            {
                var model = await characterService.GetAsync(gId);
                if (model.UserId != CurrentUserId)
                    return Forbidden();
                return View(model);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult> Add(CharacterModel model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            model.UserId = CurrentUserId;
            var result = await characterService.AddAsync(model);
            if(result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Update(CharacterModel model)
        {
            ValidateModel(model);
            if (!ModelState.IsValid)
                return Json(JsonStatuses.ValidationError, GetModelErrors());
            model.UserId = CurrentUserId;
            var result = await characterService.UpdateAsync(model);
            if(result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }

        [Route("user/characters/{type?}")]
        public ActionResult UserCharacters()
        {
            var model = new UserCharactersModel();
            return View(model);
        }

        [HttpPost]
        [Route("user/characters/{type?}")]
        public async Task<ActionResult> Characters(CharacterTopFilterModel filterModel)
        {
            var characters = await characterService.GetByFilterAsync(CurrentUserId, filterModel);
            return Json(JsonStatuses.Success, characters);
        }

        public async Task<ActionResult> Show(string id)
        {
            if(Guid.TryParse(id, out Guid gId))
            {
                var character = await characterService.GetAsync(gId);
                if (character == null)
                    return NotFound();
                return View(character);
            }
            return NotFound();
        }

        private void ValidateModel(CharacterModel model)
        {
            if (Regex.IsMatch(model.Description, @"<\s*script\s*>"))
                ModelState.AddModelError("Description", "Присутствуют запрещенные теги");

            if (model.Features == null || model.Features.Count == 0)
                ModelState.AddModelError("Description", "Добавьте хотя бы одну особенность");
        }
    }
}