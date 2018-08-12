using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class AuthorTariff : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        [Required]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }
        public User Author { get; set; }
    }
}
