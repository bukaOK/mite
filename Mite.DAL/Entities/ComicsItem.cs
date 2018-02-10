using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Guid PostId { get; set; }
        /// <summary>
        /// Страница комикса
        /// </summary>
        public int Page { get; set; }
        public string ContentSrc { get; set; }
        public string ContentSrc_50 { get; set; }
    }
}
