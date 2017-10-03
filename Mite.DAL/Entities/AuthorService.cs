using Mite.CodeData.Enums;
using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Услуги, которые могут предоставлять авторы
    /// </summary>
    public class AuthorService : GuidEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(800)]
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        /// <summary>
        /// Путь к сжатому изображению на 50L
        /// </summary>
        public string ImageSrc_50 { get; set; }
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
        /// <summary>
        /// Рейтинг
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Тип услуги(портрет, тату и т.д.)
        /// </summary>
        [ForeignKey("ServiceType")]
        public Guid ServiceTypeId { get; set; }
        /// <summary>
        /// Хранится Json представление данных для записи репоста
        /// </summary>
        [MaxLength(600)]
        public string VkRepostConditions { get; set; }
        public AuthorServiceType ServiceType { get; set; }
        [ForeignKey("Author")]
        public string AuthorId { get; set; }
        public User Author { get; set; }
    }
}