using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class ExternalLink : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Url { get; set; }
        public bool Confirmed { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
