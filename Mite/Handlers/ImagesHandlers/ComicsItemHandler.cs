using Microsoft.AspNet.Identity;
using Mite.BLL.Helpers;
using Mite.DAL.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Handlers.ImagesHandlers
{
    public class ComicsItemHandler : BaseHandler
    {
        readonly ComicsItemsRepository itemsRepository;
        readonly PostsRepository postsRepository;

        const int DefaultResizeWidth = 200;

        public ComicsItemHandler()
        {
            itemsRepository = new ComicsItemsRepository(dbContext);
            postsRepository = new PostsRepository(dbContext);
        }
        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var comIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(comIdStr, out Guid comId))
            {
                var comItem = await itemsRepository.GetAsync(comId);
                return comItem.ContentSrc;
            }
            throw new HttpException(404, "Страница не найдена");
        }

        protected async override Task HandleRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            var colIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(colIdStr, out Guid comItemId))
            {
                //Находим элемент коллекции
                var comItem = await itemsRepository.GetAsync(comItemId);
                if (comItem == null)
                    throw new HttpException(404, "Страница не найдена");
                //Находим работу этого элемента
                var post = await postsRepository.GetWithWatermarkAsync(comItem.PostId);
                if (post == null)
                    throw new HttpException(404, "Страница не найдена");

                var comImageName = Path.GetFileNameWithoutExtension(comItem.ContentSrc);
                //Получаем расширение и формируем тип ответа
                var ext = Path.GetExtension(comItem.ContentSrc);
                resp.ContentType = FilesHelper.GetContentTypeByExtension(ext);

                var watVal = req.QueryString["watermark"].ToLower();
                var resizeVal = req.QueryString["resize"].ToLower();

                if (watVal == "true" || resizeVal == "true")
                {
                    var cachedImagePath = "";
                    resp.ContentType = "image/jpeg";

                    if (resizeVal == "true" && watVal == "true")
                    {
                        if (post.WatermarkId == null)
                        {
                            throw new HttpException(404, "Страница не найдена");
                        }

                        cachedImagePath = Path.Combine(CacheFolderPath, comImageName, "wat_thumb.jpg");

                        if (!File.Exists(cachedImagePath))
                            ResizeAndDrawWat(comItem.ContentSrc, post.Watermark, cachedImagePath, DefaultResizeWidth);
                    }
                    else if (watVal == "true")
                    {
                        if (post.WatermarkId == null)
                        {
                            throw new HttpException(404, "Страница не найдена");
                        }
                        ext = Path.GetExtension(comItem.ContentSrc) == ".gif" ? "gif" : "jpg";
                        cachedImagePath = Path.Combine(CacheFolderPath, comImageName, $"wat.{ext}");
                        if (!File.Exists(cachedImagePath))
                            DrawWatermark(comItem.ContentSrc, post.Watermark, cachedImagePath);
                        if (ext == "gif")
                            resp.ContentType = "image/gif";
                    }
                    else //if (req.QueryString["resize"] == "true")
                    {
                        cachedImagePath = Path.Combine(CacheFolderPath, comImageName, "thumb.jpg");
                        if (!File.Exists(cachedImagePath))
                            Resize(comItem.ContentSrc, cachedImagePath, DefaultResizeWidth);
                    }
                    resp.WriteFile(cachedImagePath);
                }
                else
                {
                    if (post.WatermarkId != null && post.UserId != context.User.Identity.GetUserId())
                    {
                        throw new HttpException(404, "Страница не найдена");
                    }
                    resp.WriteFile(context.Server.MapPath(comItem.ContentSrc));
                }
            }
            else
            {
                throw new HttpException(404, "Страница не найдена");
            }
        }
    }
}