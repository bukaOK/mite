using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.DTO
{
    public class VkRepostDTO
    {
        public string ContainerId { get; set; }
        public string OwnerId { get; set; }
        public string PostId { get; set; }
        public string Hash { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public string ToVkCode()
        {
            return $"<div id=\"{ContainerId}\"></div><script type=\"text/javascript\">(function(d, s, id) {{ var js, fjs = d.getElementsByTagName(s)[0]; " +
                "if (d.getElementById(id)) return; js = d.createElement(s); js.id = id; js.src = \"//vk.com/js/api/openapi.js?147\"; " +
                "fjs.parentNode.insertBefore(js, fjs); }(document, 'script', 'vk_openapi_js'));  (function() {" +
                "if (!window.VK || !VK.Widgets || !VK.Widgets.Post || " +
                $"!VK.Widgets.Post(\"{ContainerId}\", {OwnerId}, {PostId}, '{Hash}')) setTimeout(arguments.callee, 50);  }}());</script>";
        }
    }
}