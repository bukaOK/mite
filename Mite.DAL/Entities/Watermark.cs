using Mite.CodeData.Enums;
using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Watermark : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public ImageGravity Gravity { get; set; }
        /// <summary>
        /// Путь к изображению
        /// </summary>
        public string VirtualPath { get; set; }
        /// <summary>
        /// Хэш(SHA-1) изображения
        /// </summary>
        [MaxLength(160)]
        [Index(IsUnique = true)]
        public string ImageHash { get; set; }
        public string Text { get; set; }
        public int? FontSize { get; set; }
        public bool? Invert { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
