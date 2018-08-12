using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    public class OrderTopDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RequestsCount { get; set; }
        public string ImageSrc { get; set; }
        /// <summary>
        /// Путь к сжатому изображению(600px)
        /// </summary>
        public string ImageSrc_600 { get; set; }
        /// <summary>
        /// Создатель заказа
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Название типа услуги
        /// </summary>
        public AuthorServiceType OrderType { get; set; }
        /// <summary>
        /// Цена
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Кол-во часов, дней и т.п.
        /// </summary>
        public int DeadlineNum { get; set; }
        /// <summary>
        /// Тип продолжительности(час, день и т.п.)
        /// </summary>
        public DurationTypes DeadlineType { get; set; }
        public OrderStatuses Status { get; set; }
    }
}
