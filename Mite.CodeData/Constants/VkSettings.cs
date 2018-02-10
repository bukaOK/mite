using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Constants
{
    public static class VkSettings
    {
#if DEBUG
        public const string AppId = "6256553";
        public const string Secret = "1SeHQqSvQ5VhJw3DrF6r";
#else
        public const string AppId = "6013159";
        public const string Secret = "bfVzsqk5oyHvlGUwLM8P";
#endif
        public const string DefaultAuthType = "Vkontakte";
        public const string GroupKey = "41a514a84656587d21c2ea497ea1495cf18c51a2581e5b836ae15a6305ac7358b1bddfd0625630a32410f";
        public const string GroupId = "143219082";
    }
}