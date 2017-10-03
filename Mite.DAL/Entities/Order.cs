using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class Order : GuidEntity
    {
        [MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        /// <summary>
        /// Путь к сжатому изображению на 50L
        /// </summary>
        public string ImageSrc_50 { get; set; }
    }
}
