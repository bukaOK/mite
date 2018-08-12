using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class PostCharacter
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        [Key, Column(Order = 1)]
        [ForeignKey("Character")]
        public Guid CharacterId { get; set; }
        public Character Character { get; set; }
    }
}
