using System;
using System.Collections.Generic;

namespace Mite.BLL.DTO
{
    public class PostDTO
    {
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
        /// <summary>
        /// Когда опубликовано
        /// </summary>
        public DateTime? PublishDate { get; set; }
        public bool Blocked { get; set; }
        public IList<TagDTO> Tags { get; set; }
        public List<CommentDTO> Comments { get; set; }
        /// <summary>
        /// Путь к обложке
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// Только для изображений, gif ли это
        /// </summary>
        public bool IsGif { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Рейтинг
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Кол-во просмотров
        /// </summary>
        public int Views { get; set; }
        /// <summary>
        /// Только для изображений, полный путь к изображению
        /// </summary>
        public string FullPath { get; set; }
        /// <summary>
        /// Показывать ли взрослый контент
        /// </summary>
        public bool ShowAdultContent { get; set; }
        public List<RatingDTO> Ratings { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}