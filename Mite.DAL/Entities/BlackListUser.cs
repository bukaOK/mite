using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Entities
{
    public class BlackListUser
    {
        /// <summary>
        /// Тот кто добавил в черный список
        /// </summary>
        [Key, ForeignKey("Caller")]
        [Column(Order = 0)]
        public string CallerId { get; set; }
        /// <summary>
        /// Кого добавили
        /// </summary>
        [Key, ForeignKey("ListedUser")]
        [Column(Order = 1)]
        public string ListedUserId { get; set; }
        public User Caller { get; set; }
        public User ListedUser { get; set; }
    }
}
