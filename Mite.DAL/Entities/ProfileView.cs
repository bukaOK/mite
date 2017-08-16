using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Сущность просмотра профиля автора
    /// </summary>
    public class ProfileView
    {
        /// <summary>
        /// Ip пользователя, который просмотрел
        /// </summary>
        [Key, Column(Order = 0)]
        public string IP { get; set; }
        /// <summary>
        /// Владелец профиля, который просмотрели
        /// </summary>
        [Key, Column(Order = 1)]
        public string UserId { get; set; }
        /// <summary>
        /// Когда просмотрели профиль
        /// </summary>
        [Key, Column(Order = 2)]
        public DateTime Date { get; set; }
    }
}