using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Initializers.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mite.DAL.Initializers
{
    internal class ExternalLinksInitializer : DBInitializer
    {
        public ExternalLinksInitializer(AppDbContext dbContext) : base(dbContext)
        {
        }

        public override bool Initialized => DbContext.ExternalLinks.Count() > 0;

        public override void Initialize()
        {
            var socialLinks = DbContext.SocialLinks.ToList();

            var extLinks = new List<ExternalLink>();
            foreach(var socLink in socialLinks)
            {
                var props = socLink.GetType().GetProperties();
                foreach(var prop in props)
                {
                    if(prop.Name != "User" && prop.Name != "UserId")
                    {
                        var domain = "";
                        var val = prop.GetValue(socLink);
                        if (val == null)
                            continue;
                        var nickname = Regex.Match(val.ToString(), @"(https?:\/\/[a-z\.]+\/)?(?<nickname>.+)").Groups["nickname"].Value;
                        switch (prop.Name)
                        {
                            case "Facebook":
                                domain = "www.facebook.com";
                                break;
                            case "Vk":
                                domain = "vk.com";
                                break;
                            case "Twitter":
                                domain = "twitter.com";
                                break;
                            case "Dribbble":
                                domain = "dribbble.com";
                                break;
                            case "ArtStation":
                                domain = "artstation.com";
                                break;
                            case "Instagram":
                                domain = "instagram.com";
                                break;
                            default:
                                continue;
                        }
                        extLinks.Add(new ExternalLink
                        {
                            Id = Guid.NewGuid(),
                            Confirmed = true,
                            UserId = socLink.UserId,
                            Url = $"https://{domain}/{nickname}"
                        });
                    }
                }
            }
            DbContext.ExternalLinks.AddRange(extLinks);
            DbContext.SaveChanges();
        }
    }
}
