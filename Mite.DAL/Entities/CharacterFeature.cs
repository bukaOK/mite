using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class CharacterFeature : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        [ForeignKey("Character")]
        public Guid CharacterId { get; set; }
        public Character Character { get; set; }
    }
}
