using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;

namespace Mite.DAL.Entities
{
    public class Tag : IEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsConfirmed { get; set; }
        public List<Post> Posts { get; set; }
        public List<User> Users { get; set; }
    }
}