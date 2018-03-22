using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class TagModel
    {
        public string Name { get; set; }
        public int? Popularity { get; set; }
    }
    public class UserTagModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Подписан ли текущий пользователь на тег
        /// </summary>
        public bool IsFollower { get; set; }
    }
}