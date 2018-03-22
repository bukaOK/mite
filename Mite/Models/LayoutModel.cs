
namespace Mite.Models
{
    public class LayoutModel
    {
        /// <summary>
        /// Подключен ли к внешним сервисам(vk, twitter, etc.)
        /// </summary>
        public bool HasExternalLogins { get; set; }
        /// <summary>
        /// Подтвердил e-mail
        /// </summary>
        public bool EmailConfirmed { get; set; }
        /// <summary>
        /// Оставил отзыв
        /// </summary>
        public bool ReviewLeft { get; set; }
        /// <summary>
        /// Кол-во новых сообщений
        /// </summary>
        public int NewMessagesCount { get; set; }
        /// <summary>
        /// Кол-во открытых сделок
        /// </summary>
        public int NewDealsCount { get; set; }
        /// <summary>
        /// Прошел ли день со дня регистрации
        /// </summary>
        public bool RegisterDayLeft { get; set; }
        /// <summary>
        /// Подписан ли на какие нибудь теги
        /// </summary>
        public bool HasTags { get; set; }
        public UserShortModel User { get; set; }
    }
}