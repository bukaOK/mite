using Mite.BLL.Services;
using Mite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    public class CitiesController : BaseController
    {
        private readonly ICityService cityService;

        public CitiesController(ICityService cityService)
        {
            
        }
        //public Task<JsonResult> BindCityToUser(string cityName)
        //{

        //}
    }
}