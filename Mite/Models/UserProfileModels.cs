﻿using Mite.CodeData.Enums;
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
        private string avatarSrc;
        public string AvatarSrc
        {
            get
            {
                return avatarSrc.Replace("\\", "/");
            }
            set
            {
                avatarSrc = value;
            }
        }
        public int Rating { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingsCount { get; set; }
        public int Reliability { get; set; }
        /// <summary>
        /// Блок "О себе"
        /// </summary>
        public string About { get; set; }
        /// <summary>
        /// Является ли текущий пользователь подписчиком
        /// пользователя страницы
        /// </summary>
        public bool IsFollowing { get; set; }
        public bool IsAuthor { get; set; }
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
        public Guid Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime LastEdit { get; set; }
        public DateTime? PublishDate { get; set; }
        public string PublicTimeStr { get; set; }
        /// <summary>
        /// Только для изображений, gif ли это
        /// </summary>
        public bool IsGif { get; set; }
        /// <summary>
        /// Только для изображений, полный путь к изображению
        /// </summary>
        public string FullPath { get; set; }
        public int CommentsCount { get; set; }
        public PostTypes Type { get; set; }
        public PostContentTypes ContentType { get; set; }
        public bool IsPublished => PublishDate != null;
        /// <summary>
        /// Может ли пользователь редактировать работу
        /// </summary>
        public bool CanEdit => (PublishDate == null) || (PublishDate != null && (DateTime.UtcNow - PublishDate).Value.TotalDays <= 3);

        private string cover;
        public string Cover
        {
            get
            {
                return cover == null ? cover : cover.Replace('\\', '/');
            }
            set
            {
                cover = value;
            }
        }

        public int Views { get; set; }
        public bool IsImage => ContentType == PostContentTypes.Image || 
            ContentType == PostContentTypes.ImageCollection;
        /// <summary>
        /// Показывать ли взрослый контент
        /// </summary>
        public bool ShowAdultContent { get; set; }
        public string PostTypeName { get; set; }
        public int Rating { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
    public class ClientProfileModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        private string avatarSrc;
        public string AvatarSrc
        {
            get
            {
                return avatarSrc.Replace("\\", "/");
            }
            set
            {
                avatarSrc = value;
            }
        }
        public string About { get; set; }
        public int Reliability { get; set; }
        /// <summary>
        /// Кол-во подписок
        /// </summary>
        public int FollowingsCount { get; set; }
        public SocialLinksModel SocialLinks { get; set; }
    }
}