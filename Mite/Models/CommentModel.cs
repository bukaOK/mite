using System;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string Content { get; set; }
        public DateTime PublicTime { get; set; }
        public int Rating { get; set; }
        /// <summary>
        /// Рейтинг текущего юзера
        /// </summary>
        public CommentRatingModel CurrentRating { get; set; }
        public CommentModel ParentComment { get; set; }
        public UserShortModel User { get; set; }
    }
}