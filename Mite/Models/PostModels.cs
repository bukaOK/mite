﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Models
{
    public class PostModel
    {
        public Guid Id { get; set; }
        [Required]
        [Display(Name = "Заголовок")]
        [MaxLength(100, ErrorMessage = "Слишком большой заголовок")]
        public string Header { get; set; }
        [AllowHtml]
        public string Content { get; set; }
        [MaxLength(350, ErrorMessage = "Слишком длинное описание")]
        public string Description { get; set; }
        public byte PostType { get; set; }
        public bool IsImage { get; set; }
        public int Views { get; set; }
        public int CommentsCount { get; set; }
        public bool IsPublished => PublishDate != null;
        public int Rating { get; set; }
        /// <summary>
        /// Рейтинг, который поставил пользователь запроса
        /// </summary>
        public PostRatingModel CurrentRating { get; set; }
        public DateTime LastEdit { get; set; }
        public DateTime? PublishDate { get; set; }
        public bool CanEdit => (PublishDate == null) || (PublishDate != null && (DateTime.UtcNow - PublishDate).Value.TotalDays <= 3);
        /// <summary>
        /// Список имен тегов
        /// </summary>
        public List<string> Tags { get; set; }
        public string Cover { get; set; }
        public bool Blocked { get; set; }
        public UserShortModel User { get; set; }
    }
    public class WritingPostModel
    {
        public Guid Id { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Заголовок")]
        [MaxLength(100, ErrorMessage = "Слишком большой заголовок")]
        [Required]
        public string Header { get; set; }
        [Required(ErrorMessage = "Вы не заполнили контент")]
        public string Content { get; set; }
        [UIHint("TextArea")]
        [DisplayName("Описание")]
        [MaxLength(350, ErrorMessage = "Слишком длинное описание")]
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public List<string> Tags { get; set; }
        public string Cover { get; set; }
        public HelperModel Helper { get; set; }
        public bool Blocked { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
    public class ImagePostModel
    {
        public Guid Id { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Заголовок")]
        [MaxLength(100, ErrorMessage = "Слишком большой заголовок")]
        [Required]
        public string Header { get; set; }
        [Required(ErrorMessage = "Вы не загрузили изображение")]
        public string Content { get; set; }
        public bool IsPublished { get; set; }
        [DisplayName("Описание")]
        [UIHint("TextArea")]
        [MaxLength(350, ErrorMessage = "Слишком длинное описание")]
        public string Description { get; set; }
        public bool Blocked { get; set; }
        public List<string> Tags { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
    public class PostRatingModel
    {
        public Guid Id { get; set; }
        [Required]
        [Range(0, 5)]
        public byte Value { get; set; }
        [Required]
        public Guid PostId { get; set; }
        public string UserId { get; set; }
    }
    /// <summary>
    /// Модель изображения в галерее (когда показывают пост)
    /// </summary>
    public class GalleryPostModel
    {
        public string Id { get; set; }
        /// <summary>
        /// Ссылка на изображение
        /// </summary>
        public string Content { get; set; }
    }
    public class TopPostModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Здесь может храниться или отрывок из документа, или путь к сжатой картинке
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Только для изображений, полный путь к изображению
        /// </summary>
        public string FullPath { get; set; }
        public bool IsImage { get; set; }
        public DateTime PublishDate { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Только для изображений, gif ли это
        /// </summary>
        public bool IsGif { get; set; }
        public string Cover { get; set; }
        public int Rating { get; set; }
        public int Views { get; set; }
        public int CommentsCount { get; set; }
        /// <summary>
        /// Показывать ли взрослый контент.
        /// </summary>
        public bool ShowAdultContent { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public UserShortModel User { get; set; }
    }
}