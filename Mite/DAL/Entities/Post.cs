using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;
using System.Collections.Generic;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Рисунок, фотография, урок и т.п.
    /// </summary>
    public class Post : IEntity
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Может храниться путь к картинке, если пост является рисунком
        /// или текст, если это рассказ, книга и т.п.
        /// </summary>
        public string Content { get; set; }
        public bool IsImage { get; set; }
        /// <summary>
        /// Время последнего редактирования
        /// </summary>
        public DateTime LastEdit { get; set; }
        public bool IsPublished { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Comment> Comments { get; set; }
        /// <summary>
        /// Путь к обложке
        /// </summary>
        public string Cover { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Для экономии добавляем int'овое значение рейтинга
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Кол-во просмотров
        /// </summary>
        public int Views { get; set; }
        public List<Rating> Ratings { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}