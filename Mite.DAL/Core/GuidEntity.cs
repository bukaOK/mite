using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Core
{
    public abstract class GuidEntity : IGuidEntity
    {
        public GuidEntity()
        {
            Id = Guid.NewGuid();
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
    }
}