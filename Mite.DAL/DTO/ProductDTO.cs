using System;

namespace Mite.DAL.DTO
{
    public class ProductDTO : PostDTO
    {
        public Guid PostId { get; set; }
        public double Price { get; set; }
    }
}
