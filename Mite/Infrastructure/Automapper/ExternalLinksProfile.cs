using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Text.RegularExpressions;

namespace Mite.Infrastructure.Automapper
{
    public class ExternalLinksProfile : Profile
    {
        public ExternalLinksProfile()
        {
            CreateMap<ExternalLinkEditModel, ExternalLink>();
            CreateMap<ExternalLink, ExternalLinkEditModel>();

            CreateMap<ExternalLinkModel, ExternalLink>()
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    if (context.Items.TryGetValue("userId", out object userId))
                        return userId as string;
                    return null;
                }))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));

            CreateMap<ExternalLink, ExternalLinkModel>()
                .ForMember(dest => dest.IconClass, opt => opt.ResolveUsing(src =>
                {
                    var match = Regex.Match(src.Url, @"^https?:\/\/(www\.)?(vk|deviantart|linkedin|instagram|dribbble|facebook|twitter|kickstarter|lastfm|github)");
                    if (match.Success && match.Groups[2].Success)
                        return match.Groups[2].Value;
                    return "external alternate";
                }))
                .ForMember(dest => dest.ShowUrl, opt => opt.MapFrom(src => Regex.Replace(src.Url, @"^(https?:\/\/)(www\.)?([\w\.]+)\.(\w+)\/(\w+)", "$3/$5")))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => "https://mitegroup.ru/away?url=" + src.Url));
        }
    }
}