using Mite.BLL.DTO;
using System;
using System.Collections.Generic;

namespace Mite.BLL.DTO
{
    public class AuthorDTO
    {
        public string Id { get; set; }

        public UserDTO User { get; set; }
        /// <summary>
        /// Показывать ли рекламу(только для авторов)
        /// </summary>
        public bool ShowAd { get; set; }
        public IList<PostDTO> Posts { get; set; }
        public IList<TagDTO> Tags { get; set; }
        public int Rating { get; set; }
        /// <summary>
        /// Подписчики пользователя
        /// </summary>
        public IList<FollowerDTO> Followers { get; set; }
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