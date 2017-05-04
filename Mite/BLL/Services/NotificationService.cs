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
using System.Linq;

namespace Mite.BLL.Services
{
    public interface INotificationService
    {
        Task AddAsync(NotificationModel notifyModel);
        /// <summary>
        /// Возвращает новые уведомления пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="onlyNew">Только новые или все</param>
        /// <returns></returns>
        Task<List<NotificationModel>> GetByUserAsync(string userId, bool onlyNew);
        /// <summary>
        /// Делаем новые уведомления устаревшими
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task ReadAsync(string userId);
        /// <summary>
        /// Удаляет все уведомления у пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task Clean(string userId);
    }
    public class NotificationService : DataService, INotificationService
    {
        public NotificationService(IUnitOfWork database) : base(database)
        {
        }

        public Task AddAsync(NotificationModel notifyModel)
        {
            notifyModel.IsNew = true;
            notifyModel.NotifyDate = DateTime.UtcNow;
            notifyModel.Id = Guid.NewGuid();

            var notify = Mapper.Map<Notification>(notifyModel);
            return Database.NotificationRepository.AddAsync(notify);
        }

        public Task Clean(string userId)
        {
            return Database.NotificationRepository.RemoveByUserAsync(userId);
        }
        public async Task<List<NotificationModel>> GetByUserAsync(string userId, bool onlyNew)
        {
            var notifications = await Database.NotificationRepository.GetByUserAsync(userId, onlyNew);
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
        
        public Task ReadAsync(string userId)
        {
            return Database.NotificationRepository.ReadByUserAsync(userId);
        }
    }
}