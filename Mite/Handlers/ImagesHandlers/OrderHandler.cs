using Mite.DAL.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Handlers.ImagesHandlers
{
    public class OrderHandler : BaseHandler
    {
        const int DefaultWidth = 400;

        private readonly OrderRepository ordersRepo;
        public OrderHandler()
        {
            ordersRepo = new OrderRepository(dbContext);
        }
        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var orderIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(orderIdStr, out Guid orderId))
            {
                var order = await ordersRepo.GetAsync(orderId);
                return order.ImageSrc;
            }
            throw new HttpException(404, "Страница не найдена");
        }

        protected async override Task HandleRequestAsync(HttpContext context)
        {
            var resp = context.Response;
            var req = context.Request;

            var serviceIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(serviceIdStr, out Guid orderId))
            {
                var order = await ordersRepo.GetAsync(orderId);
                var serviceImageName = Path.GetFileNameWithoutExtension(order.ImageSrc);

                if (req.QueryString["resize"] == "true")
                {
                    var cachedImagePath = "";
                    resp.ContentType = "image/jpeg";

                    cachedImagePath = Path.Combine(CacheFolderPath, serviceImageName, "thumb.jpg");
                    if (!File.Exists(cachedImagePath))
                        Resize(order.ImageSrc, cachedImagePath, DefaultWidth);

                    resp.WriteFile(cachedImagePath);
                }
                else
                {
                    resp.WriteFile(context.Server.MapPath(order.ImageSrc));
                }
            }
            else
            {
                throw new HttpException(404, "Страница не найдена");
            }
        }
    }
}