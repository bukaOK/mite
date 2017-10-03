using Mite.ExternalServices.VkApi.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Objects
{
    public class Post
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("owner_id")]
        public string OwnerId { get; set; }
        [JsonProperty("from_id")]
        public string FromId { get; set; }
        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
        [JsonProperty("date")]
        public uint Date { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("reply_owner_id")]
        public string ReplyOwnerId { get; set; }
        [JsonProperty("reply_post_id")]
        public string ReplyPostId { get; set; }
        [JsonProperty("friends_only")]
        [JsonConverter(typeof(BoolConverter))]
        public bool FriendsOnly { get; set; }
        [JsonProperty("comments")]
        public PostCommentsMeta PostCommentsMeta { get; set; }
        [JsonProperty("likes")]
        public PostLikesMeta PostLikesMeta { get; set; }
        [JsonProperty("reposts")]
        public PostRepostsMeta PostRepostsMeta { get; set; }
        [JsonProperty("post_type")]
        public string PostType { get; set; }
        [JsonProperty("signer_id")]
        public string SignerId { get; set; }
        [JsonProperty("copy_history")]
        public IList<Post> CopyHistory { get; set; }
        [JsonProperty("can_pin")]
        [JsonConverter(typeof(BoolConverter))]
        public bool CanPin { get; set; }
        [JsonProperty("can_delete")]
        [JsonConverter(typeof(BoolConverter))]
        public bool CanDelete { get; set; }
        [JsonProperty("can_edit")]
        [JsonConverter(typeof(BoolConverter))]
        public bool CanEdit { get; set; }
        [JsonProperty("is_pinned")]
        [JsonConverter(typeof(BoolConverter))]
        public bool IsPinned { get; set; }
        [JsonProperty("marked_as_ads")]
        [JsonConverter(typeof(BoolConverter))]
        public bool MarkedAsAds { get; set; }
    }
    public class PostCommentsMeta
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("can_post")]
        [JsonConverter(typeof(BoolConverter))]
        public bool CanPost { get; set; }
        [JsonProperty("groups_can_post ")]
        [JsonConverter(typeof(BoolConverter))]
        public bool GroupsCanPost { get; set; }
    }
    public class PostLikesMeta
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("user_likes")]
        [JsonConverter(typeof(BoolConverter))]
        public bool UserLikes { get; set; }
        [JsonProperty("can_like")]
        [JsonConverter(typeof(BoolConverter))]
        public bool CanLike { get; set; }
    }
    public class PostRepostsMeta
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("user_reposted")]
        [JsonConverter(typeof(BoolConverter))]
        public bool UserReposted { get; set; }
    }
    public class PostViewMeta
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
