using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.DTO
{
    public class UserDTO
    {
        /// <summary>
        /// Ник
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Возраст
        /// </summary>
        public byte? Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegisterDate { get; set; }
        /// <summary>
        /// Описание пользователя (блок "О себе")
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Путь к аватарке
        /// </summary>
        public string AvatarSrc { get; set; }
        /// <summary>
        /// Пол
        /// </summary>
        public byte Gender { get; set; }
        /// <summary>
        /// Местоположение(город)
        /// </summary>
        public CityDTO City { get; set; }
        public Guid? CityId { get; set; }
        /// <summary>
        /// Показывать ли рекламу(только для авторов)
        /// </summary>
        public bool ShowAd { get; set; }
        /// <summary>
        /// Рейтинг(только для авторов)
        /// </summary>
        public int Rating { get; set; }
        /// <summary>
        /// Теги пользователя
        /// </summary>
        public IList<TagDTO> Tags { get; set; }
        /// <summary>
        /// Работы пользователя(только для авторов)
        /// </summary>
        public IList<PostDTO> Posts { get; set; }
        /// <summary>
        /// Номер Яндекс кошелька
        /// </summary>
        public string YandexWalId { get; set; }
        /// <summary>
        /// Id пользователя, который пригласил данного
        /// </summary>
        public string RefererId { get; set; }
        public UserDTO Referer { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// Кол-во постов
        /// </summary>
        public int PostsCount { get; set; }
        /// <summary>
        /// Кол-во подписчиков
        /// </summary>
        public int FollowersCount { get; set; }
        public bool IsFollowing { get; set; }
    }
    public class UserProfileDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string AvatarSrc { get; set; }
        public int Rating { get; set; }
        public int FollowersCount { get; set; }
        /// <summary>
        /// Блок "О себе"
        /// </summary>
        public string About { get; set; }
        /// <summary>
        /// Является ли текущий пользователь подписчиком
        /// пользователя страницы
        /// </summary>
        public bool IsFollowing { get; set; }
        public int PostsCount { get; set; }
        public string YandexWalId { get; set; }
        public bool ShowAd { get; set; }
        public SocialLinksDTO SocialLinks { get; set; }
    }
    public class UserLoginDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
    }
    public class UserRegisterDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string RefererId { get; set; }
    }
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
                if (parameter == null)
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
