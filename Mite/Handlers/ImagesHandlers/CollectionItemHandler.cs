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
    public class CollectionItemHandler : BaseHandler
    {
        readonly CollectionItemsRepository itemsRepository;
        readonly PostsRepository postsRepository;

        public CollectionItemHandler()
        {
            itemsRepository = new CollectionItemsRepository(dbContext);
            postsRepository = new PostsRepository(dbContext);
        }
        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var colIdStr = req.Path.Split('/').Last();
            if(Guid.TryParse(colIdStr, out Guid colId))
            {
                var colItem = await itemsRepository.GetAsync(colId);
                return colItem.ContentSrc;
            }
            throw new HttpException(404, "Страница не найдена");
        }

        protected async override Task HandleRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            var colIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(colIdStr, out Guid colItemId))
            {
                //Находим элемент коллекции
                var colItem = await itemsRepository.GetAsync(colItemId);
                if (colItem == null)
                    throw new HttpException(404, "Страница не найдена");
                //Находим работу этого элемента
                var post = await postsRepository.GetWithWatermarkAsync(colItem.PostId);
                if (post == null)
                    throw new HttpException(404, "Страница не найдена");

                var colImageName = Path.GetFileNameWithoutExtension(colItem.ContentSrc);
                //Получаем расширение и формируем тип ответа
                var ext = Path.GetExtension(colItem.ContentSrc);
                resp.ContentType = FilesHelper.GetContentTypeByExtension(ext);

                //Добавлять ли водяной знак
                var watVal = req.QueryString["watermark"].ToLower();
                if (watVal == "true")
                {
                    ext = ext == ".gif" ? "gif" : "jpg";
                    if (post.WatermarkId == null)
                        throw new HttpException(404, "Страница не найдена");
                    
                    var cachedImagePath = Path.Combine(CacheFolderPath, colImageName, $"wat.{ext}");
                    if (!File.Exists(cachedImagePath))
                        DrawWatermark(colItem.ContentSrc, post.Watermark, cachedImagePath);
                    resp.WriteFile(cachedImagePath);
                }
                else
                {
                    if (post.WatermarkId != null && post.UserId != context.User.Identity.GetUserId())
                    {
                        throw new HttpException(404, "Страница не найдена");
                    }
                    resp.WriteFile(context.Server.MapPath(colItem.ContentSrc));
                }
            }
            else
            {
                throw new HttpException(404, "Страница не найдена");
            }
        }
    }
}