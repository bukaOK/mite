using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Enums
{
    public enum NotificationTypes : byte
    {
        /// <summary>
        /// Оценка поста
        /// </summary>
        PostRating,
        /// <summary>
        /// Новый подписчик
        /// </summary>
        Follower,
        /// <summary>
        /// Комментарий поста
        /// </summary>
        PostComment,
        /// <summary>
        /// Оценка коммента
        /// </summary>
        CommentRating,
        /// <summary>
        /// Ответ на комментарий
        /// </summary>
        CommentReply
    }
}