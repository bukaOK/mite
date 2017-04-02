using System;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class PostRatingModel
    {
        public Guid Id { get; set; }
        [Required]
        [Range(0, 5)]
        public byte Value { get; set; }
        [Required]
        public Guid PostId { get; set; }
        public string UserId { get; set; }
    }
}