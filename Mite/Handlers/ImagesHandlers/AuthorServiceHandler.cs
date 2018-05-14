using Mite.DAL.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Handlers.ImagesHandlers
{
    public class AuthorServiceHandler : BaseHandler
    {
        const int DefaultWidth = 500;
        private readonly AuthorServiceRepository serviceRepo;

        public AuthorServiceHandler() : base()
        {
            serviceRepo = new AuthorServiceRepository(dbContext);
        }
        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var serviceIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(serviceIdStr, out Guid serviceId))
            {
                var authorService = await serviceRepo.GetAsync(serviceId);
                return authorService.ImageSrc;
            }
            throw new HttpException(404, "Страница не найдена");
        }
        protected async override Task HandleRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            var serviceIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(serviceIdStr, out Guid serviceId))
            {
                var authorService = await serviceRepo.GetAsync(serviceId);
                var serviceImageName = Path.GetFileNameWithoutExtension(authorService.ImageSrc);

                if (req.QueryString["resize"] == "true")
                {
                    resp.ContentType = "image/jpeg";

                    var cachedImagePath = Path.Combine(CacheFolderPath, serviceImageName, "thumb.jpg");
                    if (!File.Exists(cachedImagePath))
                        Resize(authorService.ImageSrc, cachedImagePath, DefaultWidth);

                    resp.WriteFile(cachedImagePath);
                }
                else
                {
                    resp.WriteFile(context.Server.MapPath(authorService.ImageSrc));
                }
            }
            else
            {
                throw new HttpException(404, "Страница не найдена");
            }
        }
    }
}