using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.DAL.Core
{
    public abstract class Entity : IEntity
    {
        public Entity()
        {
            Id = Guid.NewGuid();
        }
        [Key]
        public Guid Id { get; set; }
    }
}