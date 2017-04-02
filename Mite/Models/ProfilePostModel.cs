using System;
using System.Collections.Generic;

namespace Mite.Models
{
    /// <summary>
    /// Пост который отображается в списке постов в профиле
    /// </summary>
    public class ProfilePostModel
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime LastEdit { get; set; }
        public string PublicTimeStr { get; set; }
        public int CommentsCount { get; set; }
        public byte PostType { get; set; }
        public bool IsPublished { get; set; }
        public int Views { get; set; }
        public bool IsImage { get; set; }
        public string PostTypeName { get; set; }
        public int Rating { get; set; }
    }
}