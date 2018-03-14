using Mite.CodeData.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Models
{
    public class ChatMemberModel
    {
        public string UserId { get; set; }
        public Guid ChatId { get; set; }
        /// <summary>
        /// Может ли текущий пользователь исключить участника из чата
        /// </summary>
        public bool CanExclude { get; set; }
        public ChatMemberStatuses Status { get; set; }
        public UserShortModel User { get; set; }
        public UserShortModel Inviter { get; set; }
    }
    public class ChatModel
    {
        public Guid Id { get; set; }
        public string CreatorId { get; set; }
        [Required]
        [DisplayName("Название")]
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        [DisplayName("Тип чата")]
        public ChatTypes ChatType { get; set; }
        [DisplayName("Максимальное количество участников")]
        public int? MaxMembersCount { get; set; }
        public UserShortModel CurrentUser { get; set; }
        public UserShortModel Companion { get; set; }
        public List<UserShortModel> Members { get; set; }
        public List<EmojiGroup> Emojies { get; set; }
        public IEnumerable<ChatMessageModel> Messages { get; set; }
    }
    public class ShortChatModel
    {
        public Guid Id { get; set; }
        public int MaxMembersCount { get; set; }
        public string CreatorId { get; set; }
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public ChatTypes ChatType { get; set; }
        public ChatMemberStatuses MemberStatus { get; set; }
        public int NewMessagesCount { get; set; }
        public ChatMessageModel LastMessage { get; set; }
    }
    public class PublicChatModel
    {
        public Guid Id { get; set; }
        public int MaxMembersCount { get; set; }
        public string CreatorId { get; set; }
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public int MembersCount { get; set; }
    }
    public class ChatMessageModel
    {
        public Guid ChatId { get; set; }
        public Guid Id { get; set; }
        public UserShortModel Sender { get; set; }
        public List<MessageAttachmentModel> Attachments { get; set; }
        public IEnumerable<HttpPostedFileBase> StreamAttachments { get; set; }
        public List<UserShortModel> Recipients { get; set; }
        /// <summary>
        /// Прочитано ли кем либо из получателей(кроме текущего пользователя)
        /// </summary>
        public bool Readed { get; set; }
        /// <summary>
        /// Прочитал ли текущий пользователь
        /// </summary>
        public bool CurrentRead { get; set; }
        [AllowHtml]
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public ShortChatModel Chat { get; set; }
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