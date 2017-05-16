using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Сущность "помощника" для пользователя(для непонятливых)
    /// Если true, значит помощник использован, и больше не нужен
    /// </summary>
    public class Helper : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Нажата ли была кнопка для редактирования документа
        /// </summary>
        public bool EditDocBtn { get; set; }
        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}