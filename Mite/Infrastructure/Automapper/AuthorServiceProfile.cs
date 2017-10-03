using AutoMapper;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.Helpers;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Mite.Infrastructure.Automapper
{
    public class AuthorServiceProfile : Profile
    {
        public AuthorServiceProfile()
        {
            CreateMap<AuthorService, AuthorServiceModel>()
                .ForMember(dest => dest.ImageBase64, opt => opt.MapFrom(src => src.ImageSrc))
                .ForMember(dest => dest.VkPostCode, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkRepostConditions))
                        return null;
                    var repost = JsonConvert.DeserializeObject<VkRepost>(src.VkRepostConditions);
                    return repost.ToVkCode();
                }));
            CreateMap<AuthorServiceModel, AuthorService>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.ImageSrc, opt => opt.Ignore())
                .ForMember(dest => dest.VkRepostConditions, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkPostCode))
                        return null;
                    var regex = new Regex(@"VK\.Widgets\.Post\(('|"")(?<id>.+)('|""),\s(?<ownerId>-?\d+),\s(?<postId>\d+),\s'(?<hash>.+)'\)");
                    var match = regex.Match(src.VkPostCode);
                    var repost = new VkRepost
                    {
                        ContainerId = match.Groups["id"].Value,
                        OwnerId = match.Groups["ownerId"].Value,
                        PostId = match.Groups["postId"].Value,
                        Hash = match.Groups["hash"].Value
                    };
                    return repost.ToJson();
                }));

            CreateMap<AuthorService, ProfileServiceModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.ImageSrc_50))
                .ForMember(dest => dest.ServiceTypeName, opt => opt.MapFrom(src => src.ServiceType.Name))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => GetDeadline(src.DeadlineNum, src.DeadlineType)));

            CreateMap<AuthorService, AuthorServiceShowModel>()
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => GetDeadline(src.DeadlineNum, src.DeadlineType)))
                .ForMember(dest => dest.VkRepostCode, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkRepostConditions))
                        return null;
                    var repost = JsonConvert.DeserializeObject<VkRepost>(src.VkRepostConditions);
                    return repost.ToVkCode();
                }));

            CreateMap<ServiceTypeModel, AuthorServiceType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<AuthorServiceType, ServiceTypeModel>();
        }
        private string GetDeadline(int num, DurationTypes type)
        {
            var words = new string[3];
            switch (type)
            {
                case DurationTypes.Day:
                    words[0] = "дней";
                    words[1] = "день";
                    words[2] = "дня";
                    break;
                case DurationTypes.Hour:
                    words[0] = "часов";
                    words[1] = "час";
                    words[2] = "часа";
                    break;
                case DurationTypes.Month:
                    words[0] = "месяцев";
                    words[1] = "месяц";
                    words[2] = "месяца";
                    break;
                case DurationTypes.Week:
                    words[0] = "недель";
                    words[1] = "неделя";
                    words[2] = "недели";
                    break;
            };
            return $"{num} {ViewHelper.GetWordCase(num, words[1], words[2], words[0])}";
        }
        public class VkRepost
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
}