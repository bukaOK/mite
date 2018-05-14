using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class Purchase
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        /// <summary>
        /// Id товара
        /// </summary>
        public Product Product { get; set; }
        [Key, Column(Order = 1)]
        [ForeignKey("Buyer")]
        public string BuyerId { get; set; }
        /// <summary>
        /// Покупатель
        /// </summary>
        public User Buyer { get; set; }
        /// <summary>
        /// Дата покупки по UTC
        /// </summary>
        public DateTime Date { get; set; }
    }
}
