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
    /// Заявка на исполнение от автора
    /// </summary>
    public class OrderRequest
    {
        /// <summary>
        /// Заказ
        /// </summary>
        [Key, ForeignKey("Order"), Column(Order = 0)]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        /// <summary>
        /// Исполнитель(автор)
        /// </summary>
        [Key, ForeignKey("Executer"), Column(Order = 1)]
        public string ExecuterId { get; set; }
        public User Executer { get; set; }
        /// <summary>
        /// Когда был подан запрос
        /// </summary>
        public DateTime RequestDate { get; set; }
    }
}
