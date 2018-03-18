using Microsoft.AspNet.Identity;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.Extensions;
using Mite.Hubs.Clients.Core;
using Mite.Models;
using Newtonsoft.Json;
using System;

namespace Mite.Hubs.Clients
{
    public class NotifyHubClient : HubClient<NotifyHub>
    {
        public void NewMessage(ChatMessageModel message)
        {
            foreach (var re in message.Recipients)
            {
                if (re.Id != message.Sender.Id)
                {
                    HubContext.Clients.Group(re.Id).addMessage(JsonConvert.SerializeObject(message));
                }
            }
        }
        /// <summary>
        /// Уведомление пользователя
        /// </summary>
        /// <param name="userId">Id пользователя, которому должно прийти уведомление</param>
        /// <param name="notificationType">Тип уведомления</param>
        /// <param name="sourceValue">Значение(имя пользователя, Id работы, Id комментария и пр.)</param>
        public void NewNotification(string userId, NotificationTypes notificationType, string sourceValue)
        {
            var currentUserId = Context.User.Identity.GetUserId();
            if (userId == currentUserId)
                return;
            var notifyModel = new NotificationModel
            {
                User = new UserShortModel
                {
                    Id = userId
                },
                NotifyUser = new UserShortModel
                {
                    Id = currentUserId,
                    AvatarSrc = Context.User.Identity.GetClaimValue(ClaimConstants.AvatarSrc),
                    UserName = Context.User.Identity.Name
                },
                NotifyDate = DateTime.UtcNow,
                NotificationType = notificationType,
                Content = $"<a href=\"/user/profile/{Context.User.Identity.Name}\">{Context.User.Identity.Name}</a> "
            };
            switch (notifyModel.NotificationType)
            {
                case NotificationTypes.CommentRating:
                    notifyModel.Content += $"оценил ваш <a href=\"/posts/showpost/{sourceValue}\">комментарий</a>.";
                    break;
                case NotificationTypes.PostComment:
                    //sourceValue like {postId}#com{comId}
                    notifyModel.Content += $"прокомментировал вашу ";
                    notifyModel.Content += $"<a href=\"/posts/showpost/{sourceValue}\">работу</a>.";
                    break;
                case NotificationTypes.PostRating:
                    //sourceValue is post id
                    notifyModel.Content += "оценил вашу ";
                    notifyModel.Content += $"<a href=\"/posts/showpost/{sourceValue}\">работу</a>.";
                    break;
                case NotificationTypes.Follower:
                    notifyModel.Content += "подписался на вас.";
                    break;
                case NotificationTypes.CommentReply:
                    notifyModel.Content += $"ответил на ваш <a href=\"{sourceValue}\">комментарий</a>.";
                    break;
                default:
                    throw new NotImplementedException("Неизвестный тип уведомления");
            }
            HubContext.Clients.Group(userId).notifyUser(JsonConvert.SerializeObject(notifyModel));
        }
    }
}