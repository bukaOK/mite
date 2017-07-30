using AutoMapper;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = "moder")]
    public class ModerController : BaseController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger logger;

        public ModerController(IUnitOfWork unitOfWork, ILogger logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }
        public ViewResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> Tags()
        {
            var tags = await unitOfWork.TagsRepository.GetAllAsync();
            return Json(JsonStatuses.Success, new
            {
                Confirmed = tags.Where(x => x.IsConfirmed),
                Unchecked = tags.Where(x => !x.Checked),
                Checked = tags.Where(x => x.Checked)
            });
        }
        [HttpPost]
        public Task BindTag(Guid fromId, Guid toId)
        {
            return unitOfWork.TagsRepository.BindAsync(fromId, toId);
        }
        [HttpPost]
        public async Task UpdatePostTags(IEnumerable<string> tagsNames, Guid postId)
        {
            var tags = Mapper.Map<List<Tag>>(tagsNames);
            await unitOfWork.TagsRepository.AddWithPostAsync(tags, postId);
        }
        [HttpPost]
        public async Task CheckTag(Guid tagId)
        {
            var tag = await unitOfWork.TagsRepository.GetAsync(tagId);
            tag.Checked = true;
            await unitOfWork.TagsRepository.UpdateAsync(tag);
        }
        /// <summary>
        /// Блокируем пост(за нарушения и пр.)
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCodeResult> BlockPost(Guid id)
        {
            var post = await unitOfWork.PostsRepository.GetAsync(id);
            post.Blocked = true;
            try
            {
                await unitOfWork.PostsRepository.UpdateAsync(post);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при блокировании поста: " + e.Message);
                return InternalServerError();
            }
        }
        public async Task<HttpStatusCodeResult> UnblockPost(Guid id)
        {
            var post = await unitOfWork.PostsRepository.GetAsync(id);
            post.Blocked = false;
            try
            {
                await unitOfWork.PostsRepository.UpdateAsync(post);
                return Ok();
            }
            catch(Exception e)
            {
                logger.Error("Ошибка при разблокировке поста: " + e.Message);
                return InternalServerError();
            }
        }
    }
}