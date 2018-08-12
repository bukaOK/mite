using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Mite.Models
{
    public class OrderEditModel
    {
        public Guid Id { get; set; }
        [Required, MaxLength(200)]
        [DisplayName("Название заказа")]
        [UIHint("TextBox")]
        public string Header { get; set; }
        [UIHint("TextArea")]
        [DisplayName("Описание заказа")]
        [Required, AllowHtml]
        public string Description { get; set; }
        public HttpPostedFileBase ImageStream { get; set; }
        public string ImageSrc { get; set; }
        public IEnumerable<SelectListItem> OrderTypes { get; set; }
        [Required]
        [DisplayName("Цена")]
        public double Price { get; set; }
        [DisplayName("Срок выполнения")]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Введите правильный срок выполнения")]
        public int DeadlineNum { get; set; }
        [Required]
        [DisplayName("Время")]
        public DurationTypes DeadlineType { get; set; }
        [Required]
        [DisplayName("Тип заказа")]
        public Guid OrderTypeId { get; set; }
        public string UserId { get; set; }
    }
    public class OrderShowModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        public string Deadline { get; set; }
        public AuthorServiceType OrderType { get; set; }
        public UserShortModel User { get; set; }
        public List<UserShortModel> Executers { get; set; }
        public double Price { get; set; }
        public bool RequestSended { get; set; }
    }
    public class OrderRequestModel
    {

    }
    public class OrderTopModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requests { get; set; }
        public AuthorServiceType OrderType { get; set; }
        public int Price { get; set; }
        public string UserName { get; set; }
        public string Deadline { get; set; }
        public string ImageSrc { get; set; }
        public OrderStatuses Status { get; set; }
    }
}