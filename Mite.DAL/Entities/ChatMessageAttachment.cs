using Mite.CodeData.Enums;
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
    public class ChatMessageAttachment : GuidEntity
    {
        [ForeignKey("Message")]
        public Guid MessageId { get; set; }
        public ChatMessage Message { get; set; }
        public AttachmentTypes Type { get; set; }
        /// <summary>
        /// Оригинальное имя файла
        /// </summary>
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Src { get; set; }
        [MaxLength(500)]
        public string CompressedSrc { get; set; }
    }
}
