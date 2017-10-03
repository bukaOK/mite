using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Mite.Models
{
    /// <summary>
    /// Модель для редактирования
    /// </summary>
    public class AuthorServiceModel
    {
        public Guid? Id { get; set; }
        [Required]
        [MaxLength(200)]
        [DisplayName("Название")]
        public string Title { get; set; }
        [MaxLength(800)]
        [DisplayName("Описание")]
        public string Description { get; set; }
        /// <summary>
        /// Содержит изоюражение в base64. 
        /// В случае редактирования содержит путь к изображению
        /// </summary>
        [DisplayName("Изображение")]
        [Required(ErrorMessage = "Выберите изображение")]
        public string ImageBase64 { get; set; }
        /// <summary>
        /// Путь к изображению(содержится только для показа сохраненного в базу изображения)
        /// </summary>
        public string ImageSrc { get; set; }
        [DisplayName("Срок выполнения")]
        [Range(1, int.MaxValue, ErrorMessage = "Введите правильный срок выполнения")]
        public int DeadlineNum { get; set; }
        [Required]
        [DisplayName("Время")]
        public DurationTypes DeadlineType { get; set; }
        [DisplayName("Цена")]
        [Range(0, double.MaxValue, ErrorMessage = "Введите правильную цену")]
        public double? Price { get; set; }
        [AllowHtml]
        [DisplayName("Репост записи ВКонтакте")]
        public string VkPostCode { get; set; }
        [Required]
        [DisplayName("Тип услуги")]
        public Guid ServiceTypeId { get; set; }
        public string AuthorId { get; set; }
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }
    }
    public class ServiceTypeModel
    {
        public Guid? Id { get; set; }
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Название")]
        [MaxLength(200)]
        public string Name { get; set; }
        public bool Confirmed { get; set; }
    }
    public class ProfileServiceModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        public string ServiceTypeName { get; set; }
        public double? Price { get; set; }
        public string Deadline { get; set; }
    }
    /// <summary>
    /// Модель для показа
    /// </summary>
    public class AuthorServiceShowModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        public string Deadline { get; set; }
        public string VkRepostCode { get; set; }
        public double? Price { get; set; }
        public ServiceTypeModel ServiceType { get; set; }
        public UserShortModel Author { get; set; }
    }
}