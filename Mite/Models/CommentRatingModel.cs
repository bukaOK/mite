using System;

namespace Mite.Models
{
    public class CommentRatingModel
    {
        public Guid Id { get; set; }
        public byte Value { get; set; }
        public Guid CommentId { get; set; }
        public string UserId { get; set; }
    }
}