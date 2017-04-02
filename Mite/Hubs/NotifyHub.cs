using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Mite.Models;
using Newtonsoft.Json;
using Mite.Extensions;
using Mite.Constants;
using Mite.Enums;
using System.Threading.Tasks;

namespace Mite.Hubs
{
    [Authorize]
    public class NotifyHub : Hub
    {
        public NotifyHub() { }
        public void NewNotification(string userId, NotificationTypes notificationType)
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
                    AvatarSrc = Context.User.GetClaimValue(ClaimConstants.AvatarSrc),
                    UserName = Context.User.Identity.Name
                },
                NotifyDate = DateTime.UtcNow,
                NotificationType = notificationType,
                Content = $"<a href=\"/user/profile/{Context.User.Identity.Name}\">{Context.User.Identity.Name}</a> "
            };
            switch (notifyModel.NotificationType)
            {
                case NotificationTypes.CommentRating:
                    notifyModel.Content += "оценил ваш комментарий.";
                    break;
                case NotificationTypes.PostComment:
                    notifyModel.Content += "прокомментировал вашу работу.";
                    break;
                case NotificationTypes.PostRating:
                    notifyModel.Content += "оценил вашу работу.";
                    break;
                case NotificationTypes.Follower:
                    notifyModel.Content += "подписался на вас.";
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