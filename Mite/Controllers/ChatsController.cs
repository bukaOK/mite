using AutoMapper;
using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Extensions;
using Mite.Helpers;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class ChatsController : BaseController
    {
        private readonly IChatService chatService;
        private readonly IChatMessagesService messagesService;
        private readonly IFollowersService followersService;
        private readonly AppUserManager userManager;

        public ChatsController(IChatService chatService, IChatMessagesService messagesService, IFollowersService followersService, 
            AppUserManager userManager)
        {
            this.chatService = chatService;
            this.messagesService = messagesService;
            this.followersService = followersService;
            this.userManager = userManager;
        }
        public async Task<ActionResult> ChatFollowers(Guid chatId)
        {
            var followers = await followersService.GetForChatAsync(chatId, CurrentUserId);
            return Json(followers, JsonRequestBehavior.AllowGet);
        }
        [Route("user/chats")]
        public ActionResult UserChats()
        {
            return View(new ChatModel
            {
                CurrentUser = new UserShortModel
                {
                    Id = User.Identity.GetUserId(),
                    UserName = User.Identity.Name,
                    AvatarSrc = User.Identity.GetClaimValue(ClaimConstants.AvatarSrc)
                },
                Emojies = EmojiHelper.GetEmojies()
            });
        }
        public ActionResult NewMessagesCount()
        {
            var count = messagesService.GetNewCount(User.Identity.GetUserId());
            return Content(count.ToString());
        }
        public async Task<ActionResult> GetByUser(Guid? writingChat)
        {
            var chats = await chatService.GetByUserAsync(User.Identity.GetUserId(), writingChat);
            if (writingChat != null && Session[SessionKeys.NewChats] is List<ChatModel> sChats && sChats.Any(x => x.Id == writingChat))
            {
                chats.Insert(0, sChats.Where(x => x.Id == writingChat).Select(x => new ShortChatModel
                {
                    Id = x.Id,
                    ImageSrc = x.ImageSrc,
                    CreatorId = x.CreatorId,
                    Name = x.Name,
                    ChatType = x.ChatType
                }).First());
            }
            //Время норм сериализуется
            return Content(JsonConvert.SerializeObject(chats), "application/json");
        }
        public async Task<ActionResult> GetPublished(int page, string input)
        {
            var chats = await chatService.GetPublishedAsync(CurrentUserId, page, input);
            return Content(JsonConvert.SerializeObject(chats), "application/json");
        }
        [HttpPost]
        public async Task<ActionResult> Update(ChatModel model)
        {
            if (model.ChatType != ChatTypes.PrivateGroup && model.ChatType != ChatTypes.Public)
                ModelState.AddModelError("ChatType", "Недопустимый тип чата");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await chatService.UpdateAsync(model);
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        [HttpPost]
        public async Task<ActionResult> Add(ChatModel model)
        {
            if (model.ChatType != ChatTypes.PrivateGroup && model.ChatType != ChatTypes.Public)
                ModelState.AddModelError("ChatType", "Недопустимый тип чата");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.Members = new List<UserShortModel>
            {
                new UserShortModel{ Id = User.Identity.GetUserId() }
            };
            var result = await chatService.CreateAsync(model);
            if (result.Succeeded)
                return Json(result.ResultData);
            return InternalServerError();
        }
        [HttpPost]
        public async Task<ActionResult> Remove(Guid id)
        {
            //Пользователь без единого сообщения создал чат и решил его удалить
            if (Session[SessionKeys.NewChats] is List<ChatModel> chats)
            {
                var existingChat = chats.FirstOrDefault(x => x.Id == id);
                if (existingChat != null)
                {
                    chats.Remove(existingChat);
                    return Ok();
                }
            }
            var result = await chatService.RemoveAsync(id, User.Identity.GetUserId());
            if (result.Succeeded)
                return Ok();
            return InternalServerError();
        }
        [HttpPost]
        public async Task<ActionResult> CreateCompanionChat(string companionId)
        {
            var existingChat = await chatService.GetByMembersAsync(new[] { companionId, CurrentUserId });

            var currentUser = await userManager.FindByIdAsync(CurrentUserId);
            var companion = await userManager.FindByIdAsync(companionId);
            if (companion == null)
                return Forbidden();

            var currentUserModel = Mapper.Map<UserShortModel>(currentUser);
            var companionModel = Mapper.Map<UserShortModel>(companion);
            var model = new ChatModel
            {
                Id = existingChat?.Id ?? Guid.NewGuid(),
                CreatorId = CurrentUserId,
                ChatType = ChatTypes.Private,
                ImageSrc = companionModel.AvatarSrc,
                Name = companionModel.UserName,
                CurrentUser = currentUserModel,
                Members = new List<UserShortModel>
                {
                    currentUserModel,
                    companionModel
                }
            };
            if (existingChat == null)
            {
                var chats = Session[SessionKeys.NewChats] as List<ChatModel>;
                if (chats == null)
                    chats = new List<ChatModel>();
                chats.Add(model);
                Session[SessionKeys.NewChats] = chats;
            }
            return Json(model);
        }
        public async Task<ActionResult> ActualFollowers()
        {
            var followers = await chatService.GetActualFollowersAsync(CurrentUserId);
            if(Session[SessionKeys.NewChats] is List<ChatModel> chats)
            {
                var chatMembersIds = chats.SelectMany(x => x.Members).GroupBy(x => x.Id).Select(x => x.Key);
                followers = followers.Where(x => !chatMembersIds.Contains(x.Id));
            }
            return Json(followers, JsonRequestBehavior.AllowGet);
        }
    }
}