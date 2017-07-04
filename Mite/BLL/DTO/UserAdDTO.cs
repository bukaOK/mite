using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.DTO
{
    /// <summary>
    /// Автор, который разрешил показывать рекламу
    /// </summary>
    public class UserAdDTO
    {
        public string Id { get; set; }
        public int Rating { get; set; }

        private int? ratingActivity;
        public int? RatingActivity
        {
            get
            {
                return ratingActivity ?? 0;
            }
            set
            {
                ratingActivity = value ?? 0;
            }
        }

        private int? commentActivity;
        public int? CommentActivity
        {
            get
            {
                return commentActivity ?? 0;
            }
            set
            {
                commentActivity = value ?? 0;
            }
        }

        public int Activity => (int)RatingActivity + (int)CommentActivity;
        /// <summary>
        /// Сколько пользователей просмотрело профиль
        /// </summary>
        public IEnumerable<ProfileView> Views { get; set; }
        private int? parameter;
        /// <summary>
        /// Из этого параметра будет считаться процент от общего дохода
        /// </summary>
        public int Parameter
        {
            get
            {
                if(parameter == null)
                {
                    parameter = (int)Math.Round(Rating * Rating * Math.Sqrt(Activity));
                }
                return (int)parameter;
            }
        }
        /// <summary>
        /// Процент от общего дохода
        /// </summary>
        public float Percent { get; set; }
    }
}