using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    public class ProductCollectionItem : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(150)]
        public string Description { get; set; }
        public string ImageSrc { get; set; }
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
