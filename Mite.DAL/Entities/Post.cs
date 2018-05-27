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
        /// Хранится путь к контенту(в случае коллекции - главное изображение)
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Сжатый контент
        /// </summary>
        [Obsolete("Есть кеш")]
        public string Content_50 { get; set; }
        /// <summary>
        /// Время последнего редактирования
        /// </summary>
        public DateTime LastEdit { get; set; }
        /// <summary>
        /// Когда опубликовано
        /// </summary>
        public DateTime? PublishDate { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Comment> Comments { get; set; }
        /// <summary>
        /// Путь к обложке
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// Сжатая обложка
        /// </summary>
        [Obsolete("Есть кеш")]
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
        /// <summary>
        /// Использовать ли водяной знак для элементов коллекции(комикса)
        /// </summary>
        public bool UseWatermarkForCols { get; set; }
        /// <summary>
        /// Элементы коллекции(если работа - коллекция)
        /// </summary>
        public List<PostCollectionItem> Collection { get; set; }
        [ForeignKey("Watermark")]
        public Guid? WatermarkId { get; set; }
        public Watermark Watermark { get; set; }
        [ForeignKey("Product")]
        public Guid? ProductId { get; set; }
        public Product Product { get; set; }
        /// <summary>
        /// Страницы комикса(если работа - комикс/манга)
        /// </summary>
        public List<ComicsItem> ComicsItems { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}