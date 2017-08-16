using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace Mite.DAL.Entities
{
    public class Comment : IGuidEntity
    {
        [System.ComponentModel.DataAnnotations.Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// Когда комментарий опубликован
        /// </summary>
        public DateTime PublicTime { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public List<Rating> Ratings { get; set; }
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("ParentComment")]
        public Guid? ParentCommentId { get; set; }
        [ForeignKey("Post")]
        public Guid? PostId { get; set; }
        [Write(false)]
        public User User { get; set; }
        [Write(false)]
        public Comment ParentComment { get; set; }
        [Write(false)]
        public Post Post { get; set; }
    }
}