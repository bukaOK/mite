using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class PostModel
    {
        public Guid Id { get; set; }
        [Required]
        [Display(Name = "Заголовок")]
        public string Header { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public byte PostType { get; set; }
        public bool IsImage { get; set; }
        public int Views { get; set; }
        public int CommentsCount { get; set; }
        public bool IsPublished { get; set; }
        public int Rating { get; set; }
        /// <summary>
        /// Рейтинг, который поставил пользователь запроса
        /// </summary>
        public PostRatingModel CurrentRating { get; set; }
        public DateTime LastEdit { get; set; }
        /// <summary>
        /// Список имен тегов
        /// </summary>
        public List<string> Tags { get; set; }
        public UserShortModel User { get; set; }
    }
}