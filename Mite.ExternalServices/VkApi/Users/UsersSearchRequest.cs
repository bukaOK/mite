using Mite.ExternalServices.VkApi.Core;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Users
{
    public class UsersSearchRequest : VkRequest<UsersSearchResponse>
    {
        public UsersSearchRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }

        [VkParam("q")]
        public string Query { get; set; }
        [VkParam("sort")]
        public int Sort { get; set; }
        [VkParam("offset")]
        public int Offset { get; set; }
        [VkParam("count")]
        public int Count { get; set; }
        [VkParam("fields")]
        public string Fields { get; set; }
        [VkParam("city")]
        public int City { get; set; }
        [VkParam("country")]
        public int Country { get; set; }
        [VkParam("hometown")]
        public string Hometown { get; set; }
        [VkParam("university")]
        public int University { get; set; }
        [VkParam("sex")]
        public int Sex { get; set; }
        [VkParam("status")]
        public int? Status { get; set; }
        [VkParam("age_from")]
        public int AgeFrom { get; set; }
        [VkParam("age_to")]
        public int AgeTo { get; set; }
        [VkParam("birth_day")]
        public int? BirthDay { get; set; }
        [VkParam("birth_month")]
        public int? BirthMonth { get; set; }
        [VkParam("birth_year")]
        public int? BirthYear { get; set; }
        [VkParam("online")]
        public int? Online { get; set; }

        public override string Method => "users.search";
    }
}
