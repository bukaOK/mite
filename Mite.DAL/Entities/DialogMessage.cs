using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class DialogMessage : GuidEntity
    {
        /// <summary>
        /// Содержание
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Дата отправки
        /// </summary>
        public DateTime SendingDateTime { get; set; }
        /// <summary>
        /// Прочитано ли
        /// </summary>
        public bool Readed { get; set; }
    }
}
