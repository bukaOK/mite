using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Friends
{
    public class FriendsGetResponse
    {
        public int Count { get; set; }
        public IEnumerable<string> Items { get; set; }
    }
}
