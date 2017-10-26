using Mite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Mite.Helpers
{
    public static class EmojiHelper
    {
        public static void InitOnion()
        {
            var path = HostingEnvironment.MapPath("~/Content/onion/");
            var files = Directory.EnumerateFiles(path);
            var onions = new List<Emoji>();
            foreach(var file in files)
            {
                onions.Add(new Emoji
                {
                    Src = path.Replace(path, "")
                });
            }
            var json = JsonConvert.SerializeObject(onions);
            using(var writer = new StreamWriter(path + "data.json"))
            {
                writer.Write(json);
            }
        }
        public static IList<EmojiGroup> GetEmojies()
        {
            var path = HostingEnvironment.MapPath("~/Content/emojies/data.json");
            IList<EmojiGroup> emojiGroups;

            using (var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                emojiGroups = JsonConvert.DeserializeObject<IList<EmojiGroup>>(json);
            }
            path = HostingEnvironment.MapPath("~/Content/onion/data.json");
            using(var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                var onions = JsonConvert.DeserializeObject<IList<Emoji>>(json);
                emojiGroups.Add(new EmojiGroup
                {
                    Name = "Onion",
                    RuName = "Onion",
                    Emojies = onions,
                    MainImage = "/Content/onion/main.png",
                    EmClass = "onion-img",
                    ColsCount = "three"
                });
            }
            return emojiGroups;
        }
    }
}