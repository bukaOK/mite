using ImageMagick;
using Mite.BLL.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Mite.Handlers.ImagesHandlers
{
    public class CommonResizeHandler : IHttpHandler
    {
        public bool IsReusable => true;
        protected virtual TimeSpan ImageMaxAge => new TimeSpan(0, 0, 60);
        protected readonly string CacheFolderPath = HostingEnvironment.MapPath("~/App_Data/images_cache/");


        public void ProcessRequest(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            
            if (int.TryParse(req["size"], out int size))
            {
                var imageSrc = context.Server.MapPath(req.QueryString["path"]);

                resp.StatusCode = 200;
                resp.Cache.SetMaxAge(ImageMaxAge);

                using (var md5 = new MD5Cng())
                {
                    var cachedImagePath = GetCachedImagePath(imageSrc, false, true, false);
                    var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(imageSrc + cachedImagePath));
                    var fileHash = string.Join("", bytes.Select(x => x.ToString("x2")));
                    if (fileHash == req.Headers["If-None-Match"])
                        resp.StatusCode = 304;
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cachedImagePath));
                        if (File.Exists(cachedImagePath))
                        {
                            resp.ContentType = FilesHelper.GetContentTypeByExtension(Path.GetExtension(cachedImagePath));
                            resp.TransmitFile(cachedImagePath);
                        }
                        else
                        {
                            var fileBytes = File.ReadAllBytes(imageSrc);
                            fileBytes = Resize(fileBytes, size);
                            File.WriteAllBytes(cachedImagePath, fileBytes);

                            resp.ContentType = "image/jpeg";
                            resp.BinaryWrite(fileBytes);
                        }
                    }
                    resp.Headers["ETag"] = fileHash;
                }
            }
            else
                throw new HttpException(404, "Not found");
        }

        private byte[] Resize(byte[] originBytes, int width, int? height = null)
        {
            //Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            using (var img = new MagickImage(originBytes))
            {
                img.Format = MagickFormat.Jpeg;
                img.ColorAlpha(new MagickColor(Color.White));
                if (height == null)
                    height = width * img.Height / img.Width;
                
                img.Resize(width, (int)height);
                return img.ToByteArray();
            }
        }
        private string GetCachedImagePath(string originPath, bool watermark, bool resize, bool blur)
        {
            string cachedImageName;
            var imageName = Path.GetFileNameWithoutExtension(originPath);
            var ext = Path.GetExtension(originPath) == ".gif" ? "gif" : "jpg";
            if (blur)
            {
                if (resize)
                    cachedImageName = "blur_thumb.jpg";
                else
                    cachedImageName = "blur.jpg";
            }
            else
            {
                if (watermark && resize)
                    cachedImageName = "wat_thumb.jpg";
                else if (watermark)
                    cachedImageName = $"wat.{ext}";
                else
                    cachedImageName = "thumb.jpg";
            }
            return Path.Combine(CacheFolderPath, imageName, "wat_thumb.jpg");
        }
    }
}