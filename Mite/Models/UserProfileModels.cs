using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class ProfileModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarSrc { get; set; }
        public int Rating { get; set; }
        public int FollowersCount { get; set; }
        /// <summary>
        /// Блок "О себе"
        /// </summary>
        public string About { get; set; }
        /// <summary>
        /// Является ли текущий пользователь подписчиком
        /// пользователя страницы
        /// </summary>
        public bool IsFollowing { get; set; }
        public int PostsCount { get; set; }
        public string YandexWalId { get; set; }
        public bool ShowAd { get; set; }
        public SocialLinksModel SocialLinks { get; set; }
    }
    /// <summary>
    /// Пост который отображается в списке постов в профиле
    /// </summary>
    public class ProfilePostModel
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime LastEdit { get; set; }
        public DateTime? PublishDate { get; set; }
        public string PublicTimeStr { get; set; }
        public int CommentsCount { get; set; }
        public byte PostType { get; set; }
        public bool IsPublished => PublishDate != null;
        /// <summary>
        /// Может ли пользователь редактировать работу
        /// </summary>
        public bool CanEdit => (PublishDate == null) || (PublishDate != null && (DateTime.UtcNow - PublishDate).Value.TotalDays <= 3);
        public string Cover { get; set; }
        public int Views { get; set; }
        public bool IsImage { get; set; }
        public string PostTypeName { get; set; }
        public int Rating { get; set; }
    }
}