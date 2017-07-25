using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class Author
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; }

        public User User { get; set; }
        /// <summary>
        /// Показывать ли рекламу(только для авторов)
        /// </summary>
        public bool ShowAd { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<Tag> Tags { get; set; }
        public int Rating { get; set; }
        /// <summary>
        /// Подписчики пользователя
        /// </summary>
        public IList<Follower> Followers { get; set; }
        /// <summary>
        /// Предоставляемые услуги(только для авторов)
        /// </summary>
        //public IList<AuthorService> Services { get; set; }
        /// <summary>
        /// Номер Яндекс кошелька
        /// </summary>
        public string YandexWalId { get; set; }
    }
}