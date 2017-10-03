using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Rating : IGuidEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public byte Value { get; set; }
        public DateTime RateDate { get; set; }
        [ForeignKey("Post")]
        public Guid? PostId { get; set; }
        [ForeignKey("Comment")]
        public Guid? CommentId { get; set; }
        [ForeignKey("AuthorService")]
        public Guid? AuthorServiceId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        /// <summary>
        /// Кто оценил
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// Пост который оценили
        /// </summary>
        public Post Post { get; set; }
        /// <summary>
        /// Комментарий который оценили
        /// </summary>
        public Comment Comment { get; set; }
        public AuthorService AuthorService { get; set; }
        /// <summary>
        /// Кого оценили
        /// </summary>
        public User Owner { get; set; }
    }
}