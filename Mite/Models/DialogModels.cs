using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class DialogModel
    {
        public Guid Id { get; set; }
        public IEnumerable<EmojiGroup> Emojies { get; set; }
        public IEnumerable<DialogMessageModel> Messages { get; set; }
    }
    public class DialogMessageModel
    {
        public UserShortModel Sender { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
    }
    public class EmojiGroup
    {
        [JsonProperty("name")]
        public string Name { get; set; }
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