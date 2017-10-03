using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;
using System.Collections.Generic;
using Mite.CodeData.Enums;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Рисунок, фотография, урок и т.п.
    /// </summary>
    public class Post : IGuidEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Хранится путь к контенту
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Сжатый контент
        /// </summary>
        public string Content_50 { get; set; }
        [Obsolete("Заменено PostContentType, после обновления удалить")]
        public bool IsImage { get; set; }
        /// <summary>
        /// Время последнего редактирования
        /// </summary>
        public DateTime LastEdit { get; set; }
        /// <summary>
        /// Когда опубликовано
        /// </summary>
        public DateTime? PublishDate { get; set; }
        [Obsolete("Заменено PublishDate, должно быть удалено")]
        public bool IsPublished { get; set; }
        [Obsolete("Заменено PostTypes, после обновления базы удалить")]
        public bool Blocked { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Comment> Comments { get; set; }
        /// <summary>
        /// Путь к обложке или к главному изображению(если коллекция)
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// Сжатая обложка
        /// </summary>
        public string Cover_50 { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Для экономии добавляем int'овое значение рейтинга
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Кол-во просмотров
        /// </summary>
        public int Views { get; set; }
        public PostTypes Type { get; set; }
        public PostContentTypes ContentType { get; set; }
        public List<Rating> Ratings { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}