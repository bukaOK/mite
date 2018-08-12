using Microsoft.AspNet.Identity;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using Mite.DAL.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.Handlers.ImagesHandlers
{
    public class PostHandler : BaseHandler
    {
        const int DefaultWidth = 500;
        private readonly PostsRepository postsRepository;
        private readonly ClientTariffRepository clientTariffRepository;

        public PostHandler() : base()
        {
            postsRepository = new PostsRepository(dbContext);
            clientTariffRepository = new ClientTariffRepository(dbContext);
        }

        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var postIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(postIdStr, out Guid postId))
            {
                var post = await postsRepository.GetAsync(postId);

                var resize = req.QueryString["resize"];
                var needResize = resize == "true";
                var needWatermark = post.WatermarkId != null;
                var needBlur = post.TariffId != null;

                return post.Content + GetCachedImagePath(post.Content, needWatermark, needResize, needBlur);
            }
            throw new HttpException(404, "Страница не найдена");
        }

        protected async override Task HandleRequestAsync(HttpContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            var postIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(postIdStr, out Guid postId))
            {
                var resize = req.QueryString["resize"];
                const int defaultWidth = 500;

                var post = await postsRepository.GetWithWatermarkAsync(postId);

                var fullImgPath = context.Server.MapPath(post.Content);

                var needResize = resize == "true";
                var needWatermark = post.WatermarkId != null;
                var needBlur = post.TariffId != null;
                if (context.User.Identity.GetUserId() == post.UserId)
                    needBlur = false;
                //Если человек оформил подписку, то не нужно смазывать
                else if (needBlur)
                {
                    var clientTariff = await clientTariffRepository.GetAsync((Guid)post.TariffId, context.User.Identity.GetUserId());
                    if (clientTariff != null && clientTariff.PayStatus == TariffStatuses.Paid)
                        needBlur = false;
                }

                if (post.ContentType == PostContentTypes.Document || !File.Exists(fullImgPath))
                    throw new HttpException(404, "Страница не найдена");

                var fileBytes = File.ReadAllBytes(fullImgPath);
                if (needBlur || needWatermark || needResize)
                {
                    var cachedImagePath = GetCachedImagePath(fullImgPath, needWatermark, needResize, needBlur);
                    var ext = Path.GetExtension(cachedImagePath);
                    var contentType = FilesHelper.GetContentTypeByExtension(ext);

                    if (File.Exists(cachedImagePath))
                    {
                        resp.ContentType = contentType;
                        resp.WriteFile(cachedImagePath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cachedImagePath));
                        if (needResize)
                            fileBytes = Resize(fileBytes, defaultWidth);
                        //Если смазываем, нет смысла еще и ватермарку добавлять
                        if (needBlur)
                            fileBytes = Blur(fileBytes);
                        else if(needWatermark)
                            fileBytes = DrawWatermark(fileBytes, post.Watermark, ext);


                        File.WriteAllBytes(cachedImagePath, fileBytes);
                        resp.ContentType = contentType;
                        resp.BinaryWrite(fileBytes);
                    }
                }
                else
                {
                    //Достаем оригинал
                    var ext = Path.GetExtension(fullImgPath);
                    resp.ContentType = FilesHelper.GetContentTypeByExtension(ext);
                    resp.WriteFile(fullImgPath);
                }
            }
            else
            {
                throw new HttpException(404, "Страница не найдена");
            }
        }
        private void NotFound(HttpContext context)
        {
            var resp = context.Response;
            resp.StatusCode = 404;
            resp.StatusDescription = "Not found";
        }
    }
}