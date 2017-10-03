using Mite.CodeData.Enums;
using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Сделка между клиентом и автором
    /// </summary>
    public class Deal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// Цена
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Дата окончания сделки по UTC
        /// </summary>
        public DateTime? Deadline { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Требования клиента
        /// </summary>
        public string Demands { get; set; }
        /// <summary>
        /// Стадия сделки
        /// </summary>
        public DealStatuses Status { get; set; }
        /// <summary>
        /// Результат работы
        /// </summary>
        [MaxLength(800)]
        public string ImageResultSrc { get; set; }
        [MaxLength(800)]
        public string ImageResultSrc_50 { get; set; }
        /// <summary>
        /// Выплачено ли
        /// </summary>
        public bool Payed { get; set; }
        /// <summary>
        /// Репостнули ли запись(если null, записи нет)
        /// </summary>
        public bool? VkReposted { get; set; }
        /// <summary>
        /// Услуга, к которой относится сделка
        /// </summary>
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }
        public AuthorService Service { get; set; }
        [ForeignKey("Client")]
        public string ClientId { get; set; }
        public User Client { get; set; }
        [ForeignKey("Author")]
        public string AuthorId { get; set; }
        public User Author { get; set; }
    }
}
