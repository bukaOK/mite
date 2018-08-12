using Mite.BLL.Services;
using Mite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class UserCharactersController : BaseController
    {
        private readonly ICharacterService characterService;

        public UserCharactersController(ICharacterService characterService)
        {
            this.characterService = characterService;
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}