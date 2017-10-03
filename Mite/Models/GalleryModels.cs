using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class GalleryModel
    {
        public int InitialIndex { get; set; }
        public GalleryItemModel[] Items { get; set; }
    }
    public class GalleryItemModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Ссылка на изображение
        /// </summary>
        public string ImageSrc { get; set; }
        public string ImageCompressed { get; set; }
    }
}