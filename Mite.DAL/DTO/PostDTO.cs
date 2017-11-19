using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;

namespace Mite.DAL.DTO
{
    public class PostDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Хранится путь к контенту(в случае коллекции - главное изображение)
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Сжатый контент
        /// </summary>
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
        /// Элементы коллекции(если работа - коллекция)
        /// </summary>
        public List<PostCollectionItem> Collection { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int CommentsCount { get; set; }
    }
}
