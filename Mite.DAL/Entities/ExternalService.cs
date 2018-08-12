using Mite.DAL.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Сущность внешних сервисов(google plus, vk, facebook etc.)
    /// </summary>
    public class ExternalService : GuidEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// Токен для внешнего сервиса(для GoogleApi,DeviantArt это может быть RefreshToken)
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Id группы или публичной страницы, которой владеет пользователь
        /// </summary>
        public string GroupId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}