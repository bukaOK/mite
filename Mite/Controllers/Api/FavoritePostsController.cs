using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mite.Controllers.Api
{
    [Authorize]
    public class FavoritePostsController : ApiController
    {
        readonly FavoritePostsRepository repo;
        readonly PostsRepository postsRepo;
        private readonly ILogger logger;

        public FavoritePostsController(IUnitOfWork unitOfWork, ILogger logger)
        {
            repo = unitOfWork.GetRepo<FavoritePostsRepository, FavoritePost>();
            postsRepo = unitOfWork.GetRepo<PostsRepository, Post>();
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IHttpActionResult> Add(FavoritePost favoritePost)
        {
            try
            {
                var post = await postsRepo.GetAsync(favoritePost.PostId);
                if (post == null || post.Type != PostTypes.Published)
                    return StatusCode(HttpStatusCode.Forbidden);
                await repo.AddAsync(favoritePost);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при добавлении работы в избранное: " + e.Message);
                return InternalServerError();
            }
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid postId, string userId)
        {
            try
            {
                var favoritePost = await repo.GetAsync(userId, postId);
                await repo.RemoveAsync(favoritePost);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при добавлении работы из избранного: " + e.Message);
                return InternalServerError();
            }
        }
    }
}