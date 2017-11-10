using Mite.CodeData.Enums;
using System;
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
        private string content;
        /// <summary>
        /// Если коллекция изображений или документ, то хранится документ, иначе изображение(это на входе)
        /// Если изображение или коллекция, то хранится изображение, иначе документ(это на выходе, т.е. при показе поста)
        /// </summary>
        [AllowHtml]
        public string Content
        {
            get
            {
                if (ContentType == PostContentTypes.Image)
                    return content == null ? content : content.Replace('\\', '/');
                return content;
            }
            set
            {
                content = value;
            }
        }
        [MaxLength(350, ErrorMessage = "Слишком длинное описание")]
        public string Description { get; set; }
        public PostTypes Type { get; set; }
        public PostContentTypes ContentType { get; set; }
        /// <summary>
        /// True, если это изображение, или коллекция изображений
        /// </summary>
        public int Views { get; set; }
        public int CommentsCount { get; set; }
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
        /// <summary>
        /// Обложка, если документ(т.е. главное текст), или главное изображение коллекции изображений.(при создании/редактировании)
        /// Документ если коллекция изображений(при показе поста)
        /// </summary>
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
        public UserShortModel User { get; set; }
        public IList<PostCollectionItemModel> Collection { get; set; }
        public IList<string> AvailableTags { get; set; }
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
        public PostTypes Type { get; set; }
        public bool IsPublished => Type == PostTypes.Published;
        public IList<string> Tags { get; set; }
        public IList<string> AvailableTags { get; set; }
        public string Cover { get; set; }
        public HelperModel Helper { get; set; }
        public bool Blocked { get; set; }
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
        public PostTypes Type { get; set; }
        public bool IsPublished => Type == PostTypes.Published;
        [DisplayName("Описание")]
        [UIHint("TextArea")]
        [MaxLength(350, ErrorMessage = "Слишком длинное описание")]
        public string Description { get; set; }
        public bool Blocked { get; set; }
        public PostContentTypes ContentType { get; set; }
        public IList<string> Tags { get; set; }
        public IList<string> AvailableTags { get; set; }
        public IList<PostCollectionItemModel> Collection { get; set; }
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
    public class PostCollectionItemModel
    {
        public Guid Id { get; set; }
        [MaxLength(300)]
        [DisplayName("Описание")]
        public string Description { get; set; }
        [DisplayName("Контент")]
        public string Content { get; set; }
        public Guid PostId { get; set; }
    }
    /// <summary>
    /// Модель изображения в галерее (когда показывают пост)
    /// </summary>
    public class GalleryPostModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Ссылка на изображение
        /// </summary>
        public string ImageSrc { get; set; }
        public string ImageCompressed { get; set; }
    }
}