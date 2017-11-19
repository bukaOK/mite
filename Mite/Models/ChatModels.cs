using Mite.CodeData.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Models
{
    public class ChatModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public UserShortModel CurrentUser { get; set; }
        public IList<UserShortModel> Members { get; set; }
        public IList<EmojiGroup> Emojies { get; set; }
        public IEnumerable<ChatMessageModel> Messages { get; set; }
    }
    public class ChatMessageModel
    {
        public Guid ChatId { get; set; }
        public Guid Id { get; set; }
        public UserShortModel Sender { get; set; }
        public IList<MessageAttachmentModel> Attachments { get; set; }
        public IEnumerable<HttpPostedFileBase> StreamAttachments { get; set; }
        public IList<UserShortModel> Recipients { get; set; }
        /// <summary>
        /// Прочитано ли кем либо из получателей(кроме текущего пользователя)
        /// </summary>
        public bool Readed { get; set; }
        [AllowHtml]
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
    }
    public class MessageAttachmentModel
    {
        public Guid Id { get; set; }
        public AttachmentTypes Type { get; set; }
        public string Src { get; set; }
        public string CompressedSrc { get; set; }
        public string Name { get; set; }
    }
    /*EMOJI*/
    public class EmojiGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ru_name")]
        public string RuName { get; set; }
        [JsonProperty("icon_class")]
        public string IconClass { get; set; }
        private string emClass = "em-img";
        public string EmClass { get { return emClass; } set { emClass = value; } }
        public string MainImage { get; set; }
        private string colsCount = "seven";
        public string ColsCount { get { return colsCount; } set { colsCount = value; } }
        [JsonProperty("emojies")]
        public IList<Emoji> Emojies { get; set; }
    }
    public class Emoji
    {
        [JsonProperty("src")]
        public string Src { get; set; }
        [JsonProperty("alt")]
        public string Alt { get; set; }
    }    
}