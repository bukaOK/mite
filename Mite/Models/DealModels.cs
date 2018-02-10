using Mite.CodeData.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class DealModel
    {
        public long Id { get; set; }
        public virtual double? Price { get; set; }
        public virtual string DeadlineStr { get; set; }
        public virtual string Demands { get; set; }
        public string ImageSrc { get; set; }
        public DateTime? Deadline => DeadlineStr == null ? null : (DateTime?)DateTime.Parse(DeadlineStr);
        public UserShortModel Author { get; set; }
        public UserShortModel Client { get; set; }
        public UserShortModel Moder { get; set; }
        public ChatModel Chat { get; set; }
        public ChatModel DisputeChat { get; set; }
        [DisplayName("Отзыв")]
        public string Feedback { get; set; }
        public byte Rating { get; set; }
        public DealStatuses Status { get; set; }
        public bool Payed { get; set; }
        public bool? VkReposted { get; set; }
        public bool VkAuthenticated { get; set; }
        public AuthorServiceShowModel Service { get; set; }
    }
    public class DealAuthorModel : DealModel
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Введите корректную цену")]
        [DisplayName("Цена")]
        public override double? Price { get; set; }
        [Required]
        [DisplayName("Дата выполнения")]
        public override string DeadlineStr { get; set; }
    }
    public class DealClientModel : DealModel
    {
        [Required]
        [DisplayName("Требования")]
        public override string Demands { get; set; }
    }
    public class DealUserModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string EndDate { get; set; }
        public bool New { get; set; }
        public bool ForModer { get; set; }
        public string ImageSrc { get; set; }
    }
}