using Mite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using System.Text.RegularExpressions;
using Mite.DAL.Infrastructure;
using System.Text;

namespace Mite.Controllers
{
    public class FilesController : BaseController
    {
        public ActionResult Download(string path, string name)
        {
            var match = Regex.IsMatch(path, @"^\/Public/.+");

            var pathExt = path.Split('.').Last();
            var nameExt = path.Split('.').Last();

            if (!match || !string.Equals(pathExt, nameExt))
                return Forbidden();
            var content = System.IO.File.ReadAllBytes(HostingEnvironment.MapPath(path));
            return File(content, System.Net.Mime.MediaTypeNames.Application.Octet, name);
        }
    }
}