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
    }
}