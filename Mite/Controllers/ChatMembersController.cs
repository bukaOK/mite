using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.Core;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class ChatMembersController : BaseController
    {
        private readonly IChatMembersService membersService;

        public ChatMembersController(IChatMembersService membersService)
        {
            this.membersService = membersService;
        }
        public async Task<ActionResult> GetByChat(Guid id)
        {
            var members = await membersService.GetByChatAsync(id, User.Identity.GetUserId());
            return Json(members, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> Add(ChatMemberModel model)
        {
            if (model.UserId == CurrentUserId)
                return Forbidden();
            var result = await membersService.AddAsync(model.UserId, CurrentUserId, model.ChatId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Enter(Guid chatId)
        {
            var result = await membersService.EnterAsync(CurrentUserId, chatId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success, result.ResultData);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Exit(Guid chatId)
        {
            var result = await membersService.ExitAsync(chatId, CurrentUserId);
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
        [HttpPost]
        public async Task<ActionResult> Exclude(Guid chatId, string userId)
        {
            var result = await membersService.ExcludeAsync(chatId, userId, User.Identity.GetUserId());
            if (result.Succeeded)
                return Json(JsonStatuses.Success);
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
    }
}