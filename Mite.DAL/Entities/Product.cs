using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mite.DAL.Entities
{
    public class Product : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public double Price { get; set; }
        /// <summary>
        /// Виртуальный путь к бонус архиву
        /// </summary>
        public string BonusPath { get; set; }
        public string BonusDescription { get; set; }
        /// <summary>
        /// Товар для авторов
        /// </summary>
        public bool ForAuthors { get; set; }
        public IList<Purchase> Purchases { get; set; }
        /// <summary>
        /// Какой кол-во символов надо показывать из контента(для товаров-документов)
        /// </summary>
        public int? ContentLimit { get; set; }
        //public IList<AnonymousPurchase> AnonymousPurchases { get; set; }
        /// <summary>
        /// Привязанная работа
        /// </summary>
        //[ForeignKey("Post")]
        //public Guid PostId { get; set; }
        //public Post Post { get; set; }
    }
}
