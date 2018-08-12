using ImageMagick;
using Mite.BLL.Helpers;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Mite.Handlers.ImagesHandlers
{
    public abstract class BaseHandler : HttpTaskAsyncHandler
    {
        protected readonly AppDbContext dbContext;
        /// <summary>
        /// Длительность хранения в кэше(в секундах)
        /// </summary>
        protected virtual TimeSpan ImageMaxAge => new TimeSpan(0, 0, 60);
        protected readonly string CacheFolderPath = HostingEnvironment.MapPath("~/App_Data/images_cache/");

        public BaseHandler()
        {
            dbContext = new AppDbContext();
        }
        public override bool IsReusable => true;

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            var imageSrc = await GetOriginSrcAsync(req);
            resp.StatusCode = 200;
            resp.Cache.SetMaxAge(ImageMaxAge);

            using (var md5 = new MD5Cng())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(imageSrc));
                var fileHash = string.Join("", bytes.Select(x => x.ToString("x2")));
                if (fileHash == req.Headers["If-None-Match"])
                    resp.StatusCode = 304;
                else
                    await HandleRequestAsync(context);
                resp.Headers["ETag"] = fileHash;
            }
        }
        protected abstract Task<string> GetOriginSrcAsync(HttpRequest req);
        /// <summary>
        /// Обработка запроса
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Путь к основному изображению</returns>
        protected abstract Task HandleRequestAsync(HttpContext context);

        protected string GetCachedImagePath(string originPath, bool watermark, bool resize, bool blur)
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
            return Path.Combine(CacheFolderPath, imageName, cachedImageName);
        }

        protected byte[] Resize(byte[] originBytes, int width, int? height = null)
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

        protected byte[] DrawWatermark(byte[] originBytes, Watermark wat, string extension)
        {
            //Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            var gr = (Gravity)((int)wat.Gravity);

            if (extension == ".gif" || extension == "gif")
            {
                using (var coll = new MagickImageCollection(originBytes))
                {
                    //Рисуем водяной знак
                    using (var watImg = string.IsNullOrEmpty(wat.VirtualPath)
                        ? new MagickImage(ImagesHelper.DrawWatermark(wat.FontSize ?? 0, wat.Text, wat.Invert ?? false))
                        : new MagickImage(HostingEnvironment.MapPath(wat.VirtualPath)))
                    {
                        foreach (var img in coll)
                            img.Composite(watImg, gr, CompositeOperator.Over);
                    }
                    //coll.Write(savePath);
                    return coll.ToByteArray();
                }
            }
            else
            {
                using (var img = new MagickImage(originBytes))
                {
                    img.Format = MagickFormat.Jpeg;
                    //Рисуем водяной знак
                    using (var watImg = string.IsNullOrEmpty(wat.VirtualPath)
                        ? new MagickImage(ImagesHelper.DrawWatermark(wat.FontSize ?? 0, wat.Text, wat.Invert ?? false))
                        : new MagickImage(HostingEnvironment.MapPath(wat.VirtualPath)))
                    {
                        img.Composite(watImg, gr, CompositeOperator.Over);
                    }
                    //img.Write(savePath);
                    return img.ToByteArray();
                }
            }
        }

        protected byte[] Blur(byte[] originBytes)
        {
            const int coef = 10000;
            using (var img = new MagickImage(originBytes))
            {
                img.Format = MagickFormat.Jpeg;
                var originalWidth = img.Width;
                var originalHeight = img.Height;
                img.Scale(new Percentage(20));
                img.Blur(0, 10);
                img.Scale(new Percentage(500));

                return img.ToByteArray();
            }
        }
    }
}