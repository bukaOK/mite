using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize(Roles = RoleNames.Moderator)]
    public class ModerController : BaseController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger logger;
        private readonly AppUserManager userManager;

        public ModerController(IUnitOfWork unitOfWork, ILogger logger, AppUserManager userManager)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.userManager = userManager;
        }
        public ViewResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> Tags()
        {
            var repo = unitOfWork.GetRepo<TagsRepository, Tag>();
            var tags = await repo.GetAllAsync();
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
            var repo = unitOfWork.GetRepo<TagsRepository, Tag>();
            return repo.BindAsync(fromId, toId);
        }
        [HttpPost]
        public async Task UpdatePostTags(IEnumerable<string> tagsNames, Guid postId)
        {
            var repo = unitOfWork.GetRepo<TagsRepository, Tag>();
            var tags = Mapper.Map<List<Tag>>(tagsNames);
            await repo.AddWithPostAsync(tags, postId);
        }
        [HttpPost]
        public async Task CheckTag(Guid tagId)
        {
            var repo = unitOfWork.GetRepo<TagsRepository, Tag>();
            var tag = await repo.GetAsync(tagId);
            tag.Checked = true;
            await repo.UpdateAsync(tag);
        }
        /// <summary>
        /// Привязать модератора к сделке
        /// </summary>
        /// <param name="id">id сделки</param>
        /// <returns></returns>
        public async Task<ActionResult> BindToDeal(long id)
        {
            var repo = unitOfWork.GetRepo<DealRepository, Deal>();
            var chatRepo = unitOfWork.GetRepo<ChatRepository, Chat>();

            var deal = await repo.GetAsync(id);
            if (deal.DisputeChatId == null || deal.ModerId != null)
                return Forbidden();
            var moder = await userManager.FindByIdAsync(User.Identity.GetUserId());
            var chat = await chatRepo.GetWithMembersAsync((Guid)deal.DisputeChatId);
            if (chat.Members.Any(x => x.Id == moder.Id))
                return Forbidden();
            chat.Members.Add(moder);
            try
            {
                deal.ModerId = User.Identity.GetUserId();
                await repo.UpdateAsync(deal);
                await chatRepo.UpdateAsync(chat);
                return RedirectToAction("Show", "Deals", new { id = id });
            }
            catch(Exception e)
            {
                logger.Error($"Id: {id}. Ошибка при привязке сделки: {e.Message}");
                return InternalServerError("Ошибка при привязке сделки");
            }
        }
        /// <summary>
        /// Блокируем пост(за нарушения и пр.)
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCodeResult> BlockPost(Guid id)
        {
            var repo = unitOfWork.GetRepo<PostsRepository, Post>();
            var post = await repo.GetAsync(id);
            post.Type = PostTypes.Blocked;
            try
            {
                await repo.UpdateAsync(post);
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
            var repo = unitOfWork.GetRepo<PostsRepository, Post>();

            var post = await repo.GetAsync(id);
            post.Type = PostTypes.Published;
            try
            {
                await repo.UpdateAsync(post);
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