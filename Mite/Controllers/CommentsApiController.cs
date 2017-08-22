using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;

namespace Mite.Controllers
{
    [Authorize]
    [Route("api/comments/{id?}")]
    public class CommentsApiController : ApiController
    {
        private readonly ICommentsService _commentsService;

        public CommentsApiController(ICommentsService commentsService)
        {
            _commentsService = commentsService;
        }
        // GET api/<controller>/5
        /// <summary>
        /// Достает комментарии по id поста, странице и фильтру
        /// </summary>
        /// <param name="id">id поста</param>
        /// <param name="sort">тип сортировки</param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<CommentModel>> GetByPost(Guid postId)
        {
            var comments = await _commentsService.GetCommentsByPostAsync(postId, User.Identity.GetUserId());
            return comments;
        }

        /// <summary>
        /// Добавляем коммент
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async Task<IHttpActionResult> Add(CommentModel model)
        {
            if (model.Content == null || model.PostId == null)
                return BadRequest();

            model.User = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            var result = await _commentsService.AddCommentToPostAsync(model);
            return Json(result);
        }

        [HttpPut]
        public async Task<IHttpActionResult> Update(CommentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            model.User.Id = User.Identity.GetUserId();
            await _commentsService.UpdateCommentAsync(model);
            return Ok();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            try
            {
                var commentUserId = await _commentsService.GetCommentUserIdAsync(id);
                if (commentUserId != User.Identity.GetUserId())
                    return StatusCode(HttpStatusCode.Forbidden);

                await _commentsService.DeleteCommentAsync(id);
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}