using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.Models;
using AutoMapper;
using Mite.DAL.Entities;
using Mite.Enums;
using System.Text;
using Mite.Hubs;
using Mite.BLL.IdentityManagers;
using System.Linq;

namespace Mite.BLL.Services
{
    public interface INotificationService
    {
        Task AddNotification(NotificationModel notifyModel);
        Task<List<NotificationModel>> GetNotificationsByUser(string userId);
        /// <summary>
        /// Делаем новые уведомления устаревшими
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task ReadNotificationsAsync(string userId);
        Task<int> GetNewNotificationsCountAsync(string userId);
    }
    public class NotificationService : DataService, INotificationService
    {
        public NotificationService(IUnitOfWork database) : base(database)
        {
        }

        public Task AddNotification(NotificationModel notifyModel)
        {
            notifyModel.IsNew = true;
            notifyModel.NotifyDate = DateTime.UtcNow;
            notifyModel.Id = Guid.NewGuid();

            var notify = Mapper.Map<Notification>(notifyModel);
            return Database.NotificationRepository.AddAsync(notify);
        }

        public Task<int> GetNewNotificationsCountAsync(string userId)
        {
            return Database.NotificationRepository.GetNewNotificationsCountAsync(userId);
        }

        public async Task<List<NotificationModel>> GetNotificationsByUser(string userId)
        {
            var notifications = await Database.NotificationRepository.GetNewNotificationsByUserAsync(userId);
            var notificationModels = Mapper.Map<List<NotificationModel>>(notifications);
            var content = new StringBuilder();
            foreach(var notifyModel in notificationModels)
            {
                content.AppendFormat("<a href=\"/user/profile/{0}\">{0}</a> ", notifyModel.NotifyUser.UserName);
                switch (notifyModel.NotificationType)
                {
                    case NotificationTypes.CommentRating:
                        content.Append("оценил ваш комментарий.");
                        break;
                    case NotificationTypes.PostComment:
                        content.Append("прокомментировал вашу работу.");
                        break;
                    case NotificationTypes.PostRating:
                        content.Append("оценил вашу работу.");
                        break;
                    case NotificationTypes.Follower:
                        content.Append("подписался на вас.");
                        break;
                    default:
                        throw new NotImplementedException("Неизвестный тип уведомления");
                }
                notifyModel.Content = content.ToString();
                content.Clear();
            }
            return notificationModels.OrderByDescending(x => x.NotifyDate).ToList();
        }
        public Task ReadNotificationsAsync(string userId)
        {
            return Database.NotificationRepository.ReadByUserAsync(userId);
        }
    }
}