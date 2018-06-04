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
        private readonly PostsRepository postsRepo;

        public PostHandler() : base()
        {
            postsRepo = new PostsRepository(dbContext);
        }

        protected async override Task<string> GetOriginSrcAsync(HttpRequest req)
        {
            var postIdStr = req.Path.Split('/').Last();
            if (Guid.TryParse(postIdStr, out Guid postId))
            {
                var post = await postsRepo.GetAsync(postId);
                return post.Content;
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
                var post = await postsRepo.GetWithWatermarkAsync(postId);
                var postImageName = Path.GetFileNameWithoutExtension(post.Content);

                var watVal = req.QueryString["watermark"].ToLower();
                var resizeVal = req.QueryString["resize"]?.ToLower();

                if (post.ContentType == PostContentTypes.Document || !File.Exists(context.Server.MapPath(post.Content)))
                {
                    throw new HttpException(404, "Страница не найдена");
                }

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

                        cachedImagePath = Path.Combine(CacheFolderPath, postImageName, "wat_thumb.jpg");

                        if (!File.Exists(cachedImagePath))
                            ResizeAndDrawWat(post.Content, post.Watermark, cachedImagePath, DefaultWidth);
                    }
                    else if (watVal == "true")
                    {
                        if (post.WatermarkId == null)
                        {
                            throw new HttpException(404, "Страница не найдена");
                        }
                        var ext = Path.GetExtension(post.Content) == ".gif" ? "gif" : "jpg";
                        cachedImagePath = Path.Combine(CacheFolderPath, postImageName, $"wat.{ext}");
                        if (!File.Exists(cachedImagePath))
                            DrawWatermark(post.Content, post.Watermark, cachedImagePath);
                        if (ext == "gif")
                            resp.ContentType = "image/gif";
                    }
                    else //if (req.QueryString["resize"] == "true")
                    {
                        cachedImagePath = Path.Combine(CacheFolderPath, postImageName, "thumb.jpg");
                        if (!File.Exists(cachedImagePath))
                            Resize(post.Content, cachedImagePath, DefaultWidth);
                    }
                    resp.WriteFile(cachedImagePath);
                }
                else
                {
                    if(post.WatermarkId != null && post.UserId != context.User.Identity.GetUserId())
                    {
                        throw new HttpException(404, "Страница не найдена");
                    }
                    resp.ContentType = FilesHelper.GetContentTypeByExtension(Path.GetExtension(post.Content));
                    resp.WriteFile(context.Server.MapPath(post.Content));
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