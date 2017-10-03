using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class Dialog : GuidEntity
    {
        /// <summary>
        /// Отправитель
        /// </summary>
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        public User Sender { get; set; }
        /// <summary>
        /// Получатель
        /// </summary>
        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }
        public IList<DialogMessage> Messages { get; set; }
    }
}
