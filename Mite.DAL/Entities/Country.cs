using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mite.DAL.Entities
{
    public class Country : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Формат страны в ISO 3166(для России - RU)
        /// </summary>
        [MaxLength(2)]
        public string IsoCode { get; set; }
    }
}
