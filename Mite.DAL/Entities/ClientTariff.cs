using Mite.CodeData.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class ClientTariff
    {
        [Key, ForeignKey("Client"), Column(Order = 0)]
        public string ClientId { get; set; }
        public User Client { get; set; }
        [Key, ForeignKey("Tariff"), Column(Order = 1)]
        public Guid TariffId { get; set; }
        public AuthorTariff Tariff { get; set; }
        /// <summary>
        /// Когда в последний раз оплатили
        /// </summary>
        public DateTime LastPayTimeUtc { get; set; }
        /// <summary>
        /// Статус оплаты
        /// </summary>
        public TariffStatuses PayStatus { get; set; }
    }
}
