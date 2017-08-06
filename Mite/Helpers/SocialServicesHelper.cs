using Mite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Mite.Helpers
{
    public static class SocialServicesHelper
    {
        private static Dictionary<string, string> BaseLinks
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "vk", "https://vk.com/" },
                    { "dribbble", "https://dribbble.com/" },
                    { "artstation",  "https://www.artstation.com/artist/"},
                    { "facebook", "https://facebook.com/" },
                    { "twitter", "https://twitter.com/" },
                    { "instagram", "https://instagram.com/" }
                };
            }
        }

        public static MvcHtmlString RenderSocialLinksForProfile(SocialLinksModel socialLinks)
        {
            var stringBuilder = new StringBuilder();
            var props = socialLinks.GetType().GetProperties();

            foreach(var prop in props)
            {
                var serviceName = prop.Name.ToLower();
                var serviceVal = prop.GetValue(socialLinks) as string;

                if (!string.IsNullOrEmpty(serviceVal))
                {
                    var baseLink = BaseLinks[serviceName];
                    string fullLink;

                    //Если базовая ссылка уже содержится в значении сервиса(тупые юзеры)
                    if (serviceVal.Contains(baseLink))
                        fullLink = baseLink;
                    else
                        fullLink = baseLink + serviceVal;

                    var serviceIcon = GetServiceIcon(serviceName);
                    string socialButtonContent;

                    //Если у сервиса есть базовая иконка в semantic
                    if (!string.IsNullOrEmpty(serviceIcon))
                        socialButtonContent = $"<i class=\"{serviceIcon} icon\"></i>";
                    else
                       socialButtonContent = $"<img src=\"{GetServiceImageSrc(serviceName)}\" />";

                    stringBuilder.AppendFormat("<a href=\"{0}\" class=\"ui circular {1} icon button\" target=\"_blank\">{2}</a>",
                            fullLink, serviceName, socialButtonContent);
                }
            }
            return new MvcHtmlString(stringBuilder.ToString());
        }
        public static MvcHtmlString RenderSocialServiceInputs(HtmlHelper htmlHelper, SocialLinksModel socialLinks)
        {
            var stringBuilder = new StringBuilder();
            var props = socialLinks.GetType().GetProperties();
            
            foreach(var prop in props)
            {
                var serviceName = prop.Name;
                stringBuilder.Append(htmlHelper.Editor(serviceName, new { label = BaseLinks[serviceName.ToLower()] }).ToHtmlString());
            }
            return new MvcHtmlString(stringBuilder.ToString());
        }
        /// <summary>
        /// Возвращаем класс иконки для semantic
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static string GetServiceIcon(string serviceName)
        {
            if(serviceName == "dribbble")
            {
                return "dribble";
            }
            var existingIcons = new []{ "vk", "facebook",  "twitter", "instagram" };
            if (existingIcons.Contains(serviceName))
            {
                return serviceName;
            }
            return null;
        }
        private static string GetServiceImageSrc(string serviceName)
        {
            switch (serviceName.ToLower())
            {
                case "artstation":
                    return "http://www.wekoworks.com/images/FotterIcons/artstation_logo.png";
                default:
                    return null;
            }
        }
    }
}