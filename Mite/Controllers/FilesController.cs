using Mite.Core;
using System.Linq;
using System.Web.Mvc;
using System.Web.Hosting;
using System.Text.RegularExpressions;
using System.Web;
using ImageMagick;
using System.IO;
using System.Threading.Tasks;

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
        public async Task<ActionResult> PsdToJpeg(HttpPostedFileBase psd)
        {
            var base64 = "data:image/jpeg;base64,";
            using(var mStream = new MemoryStream())
            {
                await psd.InputStream.CopyToAsync(mStream);
                mStream.Position = 0;
                using (var img = new MagickImage(mStream))
                {
                    img.Format = MagickFormat.Jpeg;
                    base64 += img.ToBase64();
                }
            }
            return Content(base64, "text/plain");
        }
        [HttpPost]
        public ActionResult UploadDocImg(HttpPostedFileBase file, int? maxWidth)
        {
            var base64 = "data:image/jpeg;base64,";
            using (var img = new MagickImage(file.InputStream))
            {
                img.Format = MagickFormat.Jpeg;
                img.ColorAlpha(new MagickColor(System.Drawing.Color.White));
                img.Quality = 75;
                if (maxWidth != null && maxWidth < img.Width)
                {
                    var height = (int)maxWidth * img.Height / img.Width;
                    img.Resize((int)maxWidth, height);
                }
                base64 += img.ToBase64();
            }
            return Content($"{{\"default\": \"{base64}\"}}");
        }
    }
}