using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Отзыв пользователя о сервисе
    /// </summary>
    public class UserReview : IGuidEntity
    {
        public Guid Id { get; set; }
        public string Review { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
