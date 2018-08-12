using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Персонаж, добавляют в работы
    /// </summary>
    public class Character : IGuidEntity
    {
        public Guid Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public string DescriptionSrc { get; set; }
        public string ImageSrc { get; set; }
        [MaxLength(200)]
        public string Universe { get; set; }
        /// <summary>
        /// Является ли персонаж оригинальным
        /// </summary>
        public bool Original { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<CharacterFeature> Features { get; set; }
    }
}
