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

            if (req.Headers["If-None-Match"] != null)
            {
                using (var sha1 = new SHA1Managed())
                {
                    var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(imageSrc));
                    var fileHash = string.Join("", bytes.Select(x => x.ToString("x2")));
                    if (fileHash == req.Headers["If-None-Match"])
                        resp.StatusCode = 304;
                    else
                        await HandleRequestAsync(context);
                    resp.Cache.SetETag(fileHash);
                }
            }
            else
                await HandleRequestAsync(context);
        }
        protected abstract Task<string> GetOriginSrcAsync(HttpRequest req);
        /// <summary>
        /// Обработка запроса
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Путь к основному изображению</returns>
        protected abstract Task HandleRequestAsync(HttpContext context);

        /// <summary>
        /// Изменяем размер и рисуем водяной знак
        /// </summary>
        /// <param name="originPath">Относит. путь к оригиналу изображения</param>
        /// <param name="wat">Относит. путь к водяному знаку</param>
        /// <param name="savePath">Полный путь к сохранению</param>
        /// <param name="gravity">Расположение водяного знака</param>
        /// <param name="width">Ширина нового изображения</param>
        /// <param name="height">Высота нового изображения</param>
        protected void ResizeAndDrawWat(string originPath, Watermark wat, string savePath, int width, int? height = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            using (var img = new MagickImage(HostingEnvironment.MapPath(originPath)))
            {
                img.Format = MagickFormat.Jpeg;
                img.BackgroundColor = new MagickColor(Color.White);
                if (height == null)
                    height = width * img.Height / img.Width;
                img.AdaptiveResize(width, (int)height);
                var gr = (Gravity)((int)wat.Gravity);

                //Рисуем водяной знак
                using (var watImg = string.IsNullOrEmpty(wat.VirtualPath)
                    ? new MagickImage(ImagesHelper.DrawWatermark(wat.FontSize ?? 0, wat.Text, wat.Invert ?? false))
                    : new MagickImage(HostingEnvironment.MapPath(wat.VirtualPath)))
                {
                    img.Composite(watImg, gr, CompositeOperator.Over);
                }
                img.Write(savePath);
            }
        }
        protected void Resize(string originPath, string savePath, int width, int? height = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            using (var img = new MagickImage(HostingEnvironment.MapPath(originPath)))
            {
                img.Format = MagickFormat.Jpeg;
                img.BackgroundColor = new MagickColor(Color.White);
                if (height == null)
                    height = width * img.Height / img.Width;
                img.AdaptiveResize(width, (int)height);
                img.Write(savePath);
            }
        }
        protected void DrawWatermark(string originPath, Watermark wat, string savePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            var gr = (Gravity)((int)wat.Gravity);

            if (Path.GetExtension(originPath) == ".gif")
            {
                using (var coll = new MagickImageCollection(HostingEnvironment.MapPath(originPath)))
                {
                    //Рисуем водяной знак
                    using (var watImg = string.IsNullOrEmpty(wat.VirtualPath)
                        ? new MagickImage(ImagesHelper.DrawWatermark(wat.FontSize ?? 0, wat.Text, wat.Invert ?? false))
                        : new MagickImage(HostingEnvironment.MapPath(wat.VirtualPath)))
                    {
                        foreach (var img in coll)
                            img.Composite(watImg, gr, CompositeOperator.Over);
                    }
                    coll.Write(savePath);
                }
            }
            else
            {
                using (var img = new MagickImage(HostingEnvironment.MapPath(originPath)))
                {
                    //Рисуем водяной знак
                    using (var watImg = string.IsNullOrEmpty(wat.VirtualPath)
                        ? new MagickImage(ImagesHelper.DrawWatermark(wat.FontSize ?? 0, wat.Text, wat.Invert ?? false))
                        : new MagickImage(HostingEnvironment.MapPath(wat.VirtualPath)))
                    {
                        img.Composite(watImg, gr, CompositeOperator.Over);
                    }
                    img.Write(savePath);
                }
            }
        }
    }
}