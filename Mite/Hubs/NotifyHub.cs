using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Mite.Models;
using Newtonsoft.Json;
using Mite.Extensions;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using System.Threading.Tasks;

namespace Mite.Hubs
{
    [Authorize]
    public class NotifyHub : Hub
    {
        public NotifyHub() { }
        /// <summary>
        /// Уведомление пользователя
        /// </summary>
        /// <param name="userId">Id пользователя, которому должно прийти уведомление</param>
        /// <param name="notificationType">Тип уведомления</param>
        /// <param name="sourceValue">Значение(имя пользователя, Id работы, Id комментария и пр.)</param>
        public void NewNotification(string userId, NotificationTypes notificationType, string sourceValue)
        {
            if (userId == Context.User.Identity.GetUserId())
                return;
            var notifyModel = new NotificationModel
            {
                User = new UserShortModel
                {
                    Id = userId
                },
                NotifyUser = new UserShortModel
                {
                    Id = Context.User.Identity.GetUserId(),
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
            Clients.Group(userId).notifyUser(JsonConvert.SerializeObject(notifyModel));
        }
        public override Task OnConnected()
        {
            //В качестве группы у нас пользователь, т.к. он может открыть несколько вкладок
            Groups.Add(Context.ConnectionId, Context.User.Identity.GetUserId());
            return base.OnConnected();
        }
    }
}