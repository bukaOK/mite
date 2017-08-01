using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mite.DAL.Core;

namespace Mite.DAL.Entities
{
    public class Tag : IGuidEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Index(IsUnique = true)]
        [MaxLength(150)]
        public string Name { get; set; }
        /// <summary>
        /// Подтвержден ли тег
        /// </summary>
        public bool IsConfirmed { get; set; }
        /// <summary>
        /// Проверен ли тег модератором
        /// </summary>
        public bool Checked { get; set; }
        public List<Post> Posts { get; set; }
        public List<User> Users { get; set; }
    }
}