using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;

namespace Mite.Controllers.Api
{
    [Authorize]
    public class CommentsController : ApiController
    {
        private readonly ICommentsService commentsService;
        private readonly IBlackListService blackListService;

        public CommentsController(ICommentsService commentsService, IBlackListService blackListService)
        {
            this.commentsService = commentsService;
            this.blackListService = blackListService;
        }
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
            var comments = await commentsService.GetCommentsByPostAsync(postId, User.Identity.GetUserId());
            return comments;
        }

        /// <summary>
        /// Добавляем коммент
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async Task<IHttpActionResult> Add(CommentModel model)
        {
            model.User = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            if (string.IsNullOrEmpty(model.Content))
            {
                ModelState.AddModelError("Content", "Пустой комментарий");
                return BadRequest(ModelState);
            }

            if (!(await blackListService.CanCommentAsync(model)))
            {
                ModelState.AddModelError("", "Пользователь в черном списке");
                return BadRequest(ModelState);
            }

            var result = await commentsService.AddCommentToPostAsync(model);
            if(result.Succeeded)
                return Json(result.ResultData);
            return InternalServerError();
        }

        [HttpPut]
        public async Task<IHttpActionResult> Update(CommentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            model.User.Id = User.Identity.GetUserId();
            await commentsService.UpdateCommentAsync(model);
            return Ok();
        }

        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            try
            {
                var commentUserId = await commentsService.GetCommentUserIdAsync(id);
                if (commentUserId != User.Identity.GetUserId())
                    return StatusCode(HttpStatusCode.Forbidden);

                await commentsService.DeleteCommentAsync(id);
                return Ok();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}