using Mite.DAL.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Факты, советы, пасхалки(для развлечения)
    /// </summary>
    public class DailyFact : IGuidEntity
    {
        public DailyFact()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(400)]
        public string Content { get; set; }
        /// <summary>
        /// Время начала показа
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Когда показы прекращаются
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
