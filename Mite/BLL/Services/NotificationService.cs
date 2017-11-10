using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using Mite.DAL.Infrastructure;
using System.Threading.Tasks;
using Mite.Models;
using AutoMapper;
using Mite.DAL.Entities;
using Mite.CodeData.Enums;
using System.Text;
using System.Linq;
using Mite.DAL.Repositories;
using NLog;

namespace Mite.BLL.Services
{
    public interface INotificationService : IDataService
    {
        Task AddAsync(NotificationModel notifyModel);
        /// <summary>
        /// Возвращает новые уведомления пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="onlyNew">Только новые или все</param>
        /// <returns></returns>
        Task<List<NotificationModel>> GetByUserAsync(string userId, bool onlyNew);
        IList<NotificationModel> GetByUser(string userId, bool onlyNew);
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
        public NotificationService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
        }

        public Task AddAsync(NotificationModel notifyModel)
        {
            var repo = Database.GetRepo<NotificationRepository, Notification>();

            notifyModel.IsNew = true;
            notifyModel.NotifyDate = DateTime.UtcNow;
            notifyModel.Id = Guid.NewGuid();

            var notify = Mapper.Map<Notification>(notifyModel);
            return repo.AddAsync(notify);
        }

        public Task Clean(string userId)
        {
            var repo = Database.GetRepo<NotificationRepository, Notification>();
            return repo.RemoveByUserAsync(userId);
        }

        public IList<NotificationModel> GetByUser(string userId, bool onlyNew)
        {
            var repo = Database.GetRepo<NotificationRepository, Notification>();
            var notifications = repo.GetByUser(userId, onlyNew);
            var notificationModels = Mapper.Map<IList<NotificationModel>>(notifications);

            var builder = new StringBuilder();
            foreach (var notifyModel in notificationModels)
            {
                notifyModel.Content = GetContent(notifyModel, builder);
                builder.Clear();
            }
            return notificationModels;
        }

        public async Task<List<NotificationModel>> GetByUserAsync(string userId, bool onlyNew)
        {
            var repo = Database.GetRepo<NotificationRepository, Notification>();

            var notifications = await repo.GetByUserAsync(userId, onlyNew);
            var notificationModels = Mapper.Map<List<NotificationModel>>(notifications);
            var builder = new StringBuilder();
            foreach(var notifyModel in notificationModels)
            {
                notifyModel.Content = GetContent(notifyModel, builder);
                builder.Clear();
            }
            return notificationModels.OrderByDescending(x => x.NotifyDate).ToList();
        }
        /// <summary>
        /// Контент уведомления
        /// </summary>
        /// <param name="notifyModel"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        private string GetContent(NotificationModel notifyModel, StringBuilder builder)
        {
            builder.AppendFormat("<a href=\"/user/profile/{0}\">{0}</a> ", notifyModel.NotifyUser.UserName);
            switch (notifyModel.NotificationType)
            {
                case NotificationTypes.CommentRating:
                    if (!string.IsNullOrEmpty(notifyModel.SourceValue))
                        //sourceValue like {postId}#com{commentId}
                        builder.AppendFormat("оценил ваш <a href=\"/posts/showpost/{0}\">комментарий</a>.", notifyModel.SourceValue);
                    else
                        builder.Append("оценил ваш комментарий.");
                    break;
                case NotificationTypes.PostComment:
                    if (!string.IsNullOrEmpty(notifyModel.SourceValue))
                    {
                        //sourceValue like {postId}#com{commentId}
                        builder.Append("прокомментировал вашу ");
                        builder.AppendFormat("<a href=\"/posts/showpost/{0}\">работу</a>.", notifyModel.SourceValue);
                    }
                    else
                    {
                        builder.Append("прокомментировал вашу работу.");
                    }
                    break;
                case NotificationTypes.PostRating:
                    //sourceValue is post id
                    builder.Append("оценил вашу ");
                    if (!string.IsNullOrEmpty(notifyModel.SourceValue))
                        builder.AppendFormat("<a href=\"/posts/showpost/{0}\">работу</a>.", notifyModel.SourceValue);
                    else
                        builder.Append("работу.");
                    break;
                case NotificationTypes.Follower:
                    builder.Append("подписался на вас.");
                    break;
                case NotificationTypes.CommentReply:
                    builder.AppendFormat("ответил на ваш <a href=\"/posts/showpost/{0}\">комментарий</a>.", notifyModel.SourceValue);
                    break;
                default:
                    throw new NotImplementedException("Неизвестный тип уведомления");
            }
            return builder.ToString();
        }
        public Task ReadAsync(string userId)
        {
            var repo = Database.GetRepo<NotificationRepository, Notification>();
            return repo.ReadByUserAsync(userId);
        }
    }
}