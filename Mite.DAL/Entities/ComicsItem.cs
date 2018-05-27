using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Одна из страниц комикса
    /// </summary>
    public class ComicsItem : GuidEntity, IContentEntity
    {
        /// <summary>
        /// Id поста, изображение поста будет главным изображением/обложкой
        /// </summary>
        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        /// <summary>
        /// Страница комикса
        /// </summary>
        public int Page { get; set; }
        public string ContentSrc { get; set; }
        [Obsolete("Есть кеш")]
        public string ContentSrc_50 { get; set; }
    }
}
