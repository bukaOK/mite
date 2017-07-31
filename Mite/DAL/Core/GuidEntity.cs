using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.DAL.Core
{
    public abstract class GuidEntity : IGuidEntity
    {
        public GuidEntity()
        {
            Id = Guid.NewGuid();
        }
        [Key]
        public Guid Id { get; set; }
    }
}