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
    public class Order : GuidEntity
    {
        [MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        /// <summary>
        /// Путь к сжатому изображению(600px)
        /// </summary>
        public string ImageSrc_600 { get; set; }
        public OrderStatuses Status { get; set; }
        /// <summary>
        /// Создатель заказа
        /// </summary>
        [Required, ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        /// <summary>
        /// Выбранный исполнитель
        /// </summary>
        [ForeignKey("Executer")]
        public string ExecuterId { get; set; }
        public User Executer { get; set; }
        /// <summary>
        /// Типы услуг тоже самое что и типы заказов, но я ж дурак, не допер сразу
        /// </summary>
        [ForeignKey("OrderType")]
        public Guid OrderTypeId { get; set; }
        public AuthorServiceType OrderType { get; set; }
        public DateTime CreateDate { get; set; }
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
        public IEnumerable<OrderRequest> Requests { get; set; }
    }
}
