using Mite.Attributes.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class ProductTopModel : TopPostModel
    {
        public bool ForAuthor { get; set; }
        public override bool IsProduct => true;
        public Guid PostId { get; set; }
        public int Price { get; set; }
    }
    //public class ProductShowModel
    //{
    //    public Guid Id { get; set; }
    //    public string ImageSrc { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //    public IList<ProductItemModel> Items { get; set; }
    //}
    public class ProductModel
    {
        public Guid? Id { get; set; }
        public Guid PostId { get; set; }
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Цена")]
        public int? Price { get; set; }
        [DisplayName("Файл бонуса")]
        public string BonusBase64 { get; set; }
        public string BonusFormat { get; set; }
        [UIHint("TextArea")]
        [DisplayName("Описание бонуса")]
        public string BonusDescription { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Для авторов")]
        public bool ForAuthors { get; set; }
        /// <summary>
        /// Куплен ли товар текущим пользователем(только для показа)
        /// </summary>
        public bool IsBought { get; set; }
    }
    public class ProductItemModel
    {
        public Guid? Id { get; set; }
        public string Description { get; set; }
        public string ImageSrc { get; set; }
    }
}