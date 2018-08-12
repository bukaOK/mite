using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    public enum NotificationTypes : byte
    {
        /// <summary>
        /// Оценка поста
        /// </summary>
        PostRating = 0,
        /// <summary>
        /// Новый подписчик
        /// </summary>
        Follower = 1,
        /// <summary>
        /// Комментарий поста
        /// </summary>
        PostComment = 2,
        /// <summary>
        /// Оценка коммента
        /// </summary>
        CommentRating = 3,
        /// <summary>
        /// Ответ на комментарий
        /// </summary>
        CommentReply = 4,
        /// <summary>
        /// Оплата за платную подписку
        /// </summary>
        TariffPayment = 5
    }
}