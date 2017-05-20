using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    /// <summary>
    /// Модель изображения в галерее (когда показывают пост)
    /// </summary>
    public class GalleryPostModel
    {
        public string Id { get; set; }
        /// <summary>
        /// Ссылка на изображение
        /// </summary>
        public string Content { get; set; }
    }
}