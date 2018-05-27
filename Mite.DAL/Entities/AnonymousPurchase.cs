using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class AnonymousPurchase : IGuidEntity
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }
        /// <summary>
        /// Код для подтверждения владения e-mail
        /// </summary>
        [MaxLength(256)]
        public string Code { get; set; }
        /// <summary>
        /// Дата окончания срока действия кода
        /// </summary>
        public DateTime CodeExpires { get; set; }
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
