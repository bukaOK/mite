using Mite.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class TopModel
    {
        public IEnumerable<TagModel> Tags { get; set; }
    }
    public class TopPostModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Здесь может храниться или отрывок из документа, или путь к сжатой картинке
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Только для изображений, полный путь к изображению
        /// </summary>
        public string FullPath { get; set; }
        public bool IsImage { get; set; }
        public DateTime PublishDate { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Только для изображений, gif ли это
        /// </summary>
        public bool IsGif { get; set; }
        public string Cover { get; set; }
        public int Rating { get; set; }
        public int Views { get; set; }
        public int CommentsCount { get; set; }
        /// <summary>
        /// Показывать ли взрослый контент.
        /// </summary>
        public bool ShowAdultContent { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public UserShortModel User { get; set; }
    }
}