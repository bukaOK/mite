using Microsoft.AspNet.Identity;
using Mite.BLL.Services;
using Mite.CodeData.Enums;
using Mite.Core;
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
    public class NotificationController : BaseController
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }
        [Route("user/notifications/{onlyNew=false}")]
        public ActionResult UserNotifications(bool onlyNew)
        {
            var notifications = notificationService.GetByUser(User.Identity.GetUserId(), onlyNew);
            if (onlyNew)
                return PartialView("_UserNotifications", notifications);
            else
                return View("Notifications", new UserNotificationsModel
                {
                    All = notifications,
                    Comments = notifications.Where(x => x.NotificationType == NotificationTypes.PostComment
                        || x.NotificationType == NotificationTypes.CommentReply),
                    Followers = notifications.Where(x => x.NotificationType == NotificationTypes.Follower),
                    Ratings = notifications.Where(x => x.NotificationType == NotificationTypes.PostRating
                        || x.NotificationType == NotificationTypes.CommentRating)
                });
        }
    }
}