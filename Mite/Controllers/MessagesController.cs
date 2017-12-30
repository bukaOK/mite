using Microsoft.AspNet.Identity;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.Core;
using Mite.Extensions;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Mite.Controllers
{
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IChatMessagesService messagesService;

        public MessagesController(IChatMessagesService messagesService)
        {
            this.messagesService = messagesService;
        }
        [HttpPost]
        public async Task<ActionResult> Add(ChatMessageModel model)
        {
            model.Sender = new UserShortModel
            {
                Id = User.Identity.GetUserId()
            };
            var chats = Session[SessionKeys.NewChats] as List<ChatModel>;
            var inSession = chats != null && chats.Any(x => x.Id == model.ChatId);
            if (inSession)
                model.Recipients = chats.First(x => x.Id == model.ChatId).Members;

            var result = await messagesService.AddAsync(model, inSession);
            if (result.Succeeded)
            {
                var message = (ChatMessageModel)result.ResultData;
                //Имя чата для собеседника(ов)
                if (string.IsNullOrEmpty(message.Chat.Name))
                    message.Chat.Name = User.Identity.Name;
                if (string.IsNullOrEmpty(message.Chat.ImageSrc) || message.Chat.ImageSrc == PathConstants.AvatarSrc)
                    message.Chat.ImageSrc = User.Identity.GetClaimValue(ClaimConstants.AvatarSrc);
                return Json(JsonStatuses.Success, message);
            }
            return InternalServerError();
        }
    }
}