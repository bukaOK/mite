using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Элемент коллекции работы
    /// </summary>
    public class PostCollectionItem : GuidEntity
    {
        [ForeignKey("Post")]
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        public string ContentSrc { get; set; }
        public string ContentSrc_50 { get; set; }
    }
}
