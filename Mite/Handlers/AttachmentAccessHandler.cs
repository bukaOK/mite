using ImageMagick;
using Microsoft.AspNet.Identity;
using Mite.CodeData.Enums;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Mite.Handlers
{
    /// <summary>
    /// Обработчик файлов сообщений
    /// </summary>
    public class AttachmentAccessHandler : IHttpHandler
    {
        const int ThumbWidth = 300;
        TimeSpan ImageMaxAge => new TimeSpan(0, 0, 60);
        readonly string CacheFolderPath = HostingEnvironment.MapPath("~/App_Data/images_cache/");

        public bool IsReusable => true;

        private readonly ChatMessageAttachmentsRepository attachmentsRepository;
        public AttachmentAccessHandler()
        {
            attachmentsRepository = new ChatMessageAttachmentsRepository(new AppDbContext());
        }
        public void ProcessRequest(HttpContext context)
        {
            var resp = context.Response;
            var req = context.Request;

            var attachmentIdStr = req.Path.Split('/').Last();
            
            if (Guid.TryParse(attachmentIdStr, out Guid attachmentId))
            {
                var hasAccess = attachmentsRepository.HasAttachmentAccess(attachmentId, context.User.Identity.GetUserId());
                if (hasAccess)
                {
                    var attachment = attachmentsRepository.Get(attachmentId);
                    var fullPath = context.Server.MapPath(attachment.Src);

                    resp.StatusCode = 200;
                    if (attachment.Type == AttachmentTypes.Image)
                    {
                        resp.ContentType = "image/jpeg";
                        resp.Cache.SetMaxAge(ImageMaxAge);

                        if (req.Headers["If-None-Match"] != null)
                        {
                            using (var sha1 = new SHA1Managed())
                            {
                                var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(attachment.Src));
                                var fileHash = string.Join("", bytes.Select(x => x.ToString("x2")));

                                resp.Cache.SetETag(fileHash);
                                if (fileHash == req.Headers["If-None-Match"])
                                {
                                    resp.StatusCode = 304;
                                    return;
                                }
                            }
                        }
                        var imageName = Path.GetFileNameWithoutExtension(attachment.Src);
                        var cachedImagePath = Path.Combine(CacheFolderPath, imageName, "thumb.jpg");

                        if (!File.Exists(cachedImagePath))
                            Resize(attachment.Src, cachedImagePath, ThumbWidth);

                        resp.WriteFile(cachedImagePath);
                    }
                    else
                    {
                        resp.WriteFile(fullPath);
                    }
                }
            }
            else
                throw new HttpException(404, "Страница не найдена");
        }
        private void Resize(string originPath, string savePath, int width, int? height = null)
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
    }
}