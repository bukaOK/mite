using Mite.ExternalServices.VkApi.Core;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Wall
{
    public class WallPostRequest : VkRequest<WallPostResponse>
    {
        public WallPostRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        /// <summary>
        /// данный параметр учитывается, если owner_id меньше
        /// 0 (запись публикуется на стене группы). 1 — запись будет опубликована от имени группы, 0 — запись будет опубликована от имени пользователя (по умолчанию). 
        /// </summary>
        [VkParam("from_group")]
        public byte FromGroup { get; set; }
        /// <summary>
        /// Текст сообщения (является обязательным, если не задан параметр attachments) 
        /// </summary>
        [VkParam("message")]
        public string Message { get; set; }
        /// <summary>
        /// Идентификатор пользователя или сообщества, на стене которого должна быть опубликована запись. 
        /// </summary>
        [VkParam("owner_id")]
        public string OwnerId { get; set; }
        /// <summary>
        /// 1 — запись будет доступна только друзьям, 0 — всем пользователям. По умолчанию публикуемые записи доступны всем пользователям. 
        /// </summary>
        [VkParam("friends_only")]
        public byte FriendsOnly { get; set; }
        /// <summary>
        /// Список объектов, приложенных к записи и разделённых символом ",". Поле attachments представляется в формате:
        /// `type`owner_id`_media_id`,`type`owner_id`_`media_id
        /// </summary>
        [VkParam("attachments")]
        public string Attachments { get; set; }
        /// <summary>
        /// 1 — у записи, размещенной от имени сообщества, будет добавлена подпись (имя пользователя, разместившего запись), 
        /// 0 — подписи добавлено не будет. Параметр учитывается только при публикации на стене сообщества и указании параметра from_group. 
        /// По умолчанию подпись не добавляется. 
        /// </summary>
        [VkParam("signed")]
        public byte Signed { get; set; }
        public override string Method => "wall.post";
    }
    public class WallPostResponse
    {
        [VkParam("post_id")]
        public string PostId { get; set; }
    }
}
