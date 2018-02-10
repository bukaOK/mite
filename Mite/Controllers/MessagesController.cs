using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Core;
using Mite.Extensions;
using Mite.Hubs.Clients;
using Mite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var result = await messagesService.AddAsync(model, CurrentUserId, inSession);
            if (result.Succeeded)
            {
                if (inSession)
                    chats.Remove(chats.First(x => x.Id == model.ChatId));
                var message = (ChatMessageModel)result.ResultData;
                //Имя чата для собеседника(ов)
                if (string.IsNullOrEmpty(message.Chat.Name))
                    message.Chat.Name = User.Identity.Name;
                if (string.IsNullOrEmpty(message.Chat.ImageSrc) || message.Chat.ImageSrc == PathConstants.AvatarSrc)
                    message.Chat.ImageSrc = User.Identity.GetClaimValue(ClaimConstants.AvatarSrc);

                ChatHubClient.AddMessage(message);
                NotifyHubClient.NewMessage(message);
                if (message.Chat.ChatType == ChatTypes.Public)
                    ChatHubClient.AddPublicMessage(message);

                return Json(JsonStatuses.Success, message);
            }
            return Json(JsonStatuses.ValidationError, result.Errors);
        }
    }
}