using AutoMapper;
using Mite.BLL.DTO;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.Helpers;
using Mite.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class AuthorServiceProfile : Profile
    {
        const string ImagesPath = "/images/services/";
        public AuthorServiceProfile()
        {
            CreateMap<AuthorService, AuthorServiceModel>()
                .ForMember(dest => dest.ImageBase64, opt => opt.MapFrom(src => src.ImageSrc))
                .ForMember(dest => dest.VkPostCode, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkRepostConditions))
                        return null;
                    var repost = JsonConvert.DeserializeObject<VkRepostDTO>(src.VkRepostConditions);
                    return repost.ToVkCode();
                }));
            CreateMap<AuthorServiceModel, AuthorService>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageSrc, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.ResolveUsing((src, dest) =>
                {
                    return string.IsNullOrEmpty(dest?.AuthorId) ? src.AuthorId : dest.AuthorId;
                }))
                .ForMember(dest => dest.VkRepostConditions, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkPostCode))
                        return null;
                    var regex = new Regex(@"VK\.Widgets\.Post\(('|"")(?<id>.+)('|""),\s(?<ownerId>-?\d+),\s(?<postId>\d+),\s'(?<hash>.+)'\)");
                    var match = regex.Match(src.VkPostCode);
                    var repost = new VkRepostDTO
                    {
                        ContainerId = match.Groups["id"].Value,
                        OwnerId = match.Groups["ownerId"].Value,
                        PostId = match.Groups["postId"].Value,
                        Hash = match.Groups["hash"].Value
                    };
                    return repost.ToJson();
                }));

            CreateMap<AuthorServiceModel, VkServiceModel>();
            CreateMap<VkServiceModel, AuthorService>()
                .ForMember(dest => dest.AuthorId, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var currentUserId = (string)context.Items["UserId"];
                    return currentUserId;
                }))
                .ForMember(dest => dest.CreateDate, opt => opt.UseValue(DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageSrc, opt => opt.ResolveUsing((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.ImageBase64))
                    {
                        var imageSrc = FilesHelper.CreateImage(PathConstants.VirtualImageFolder, src.ImageBase64);
                        //dest.ImageSrc_50 = FilesHelper.ToVirtualPath(ImagesHelper.Resize(HostingEnvironment.MapPath(imageSrc), 500));
                        return imageSrc;
                    }
                    else
                    {
                        dest.ImageSrc_50 = src.VkThumbLink;
                        return src.VkLink;
                    }
                }))
                .ForMember(dest => dest.ImageSrc_50, opt => opt.Ignore())
                .ForMember(dest => dest.VkRepostConditions, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkPostCode))
                        return null;
                    var regex = new Regex(@"VK\.Widgets\.Post\(('|"")(?<id>.+)('|""),\s(?<ownerId>-?\d+),\s(?<postId>\d+),\s'(?<hash>.+)'\)");
                    var match = regex.Match(src.VkPostCode);
                    var repost = new VkRepostDTO
                    {
                        ContainerId = match.Groups["id"].Value,
                        OwnerId = match.Groups["ownerId"].Value,
                        PostId = match.Groups["postId"].Value,
                        Hash = match.Groups["hash"].Value
                    };
                    return repost.ToJson();
                }));
            CreateMap<AuthorService, ProfileServiceModel>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Length > 300 ? 
                    src.Description.Substring(0, 300) + "..." : src.Description))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => GetImageSrc(src.Id, src.ImageSrc, src.ImageSrc_50, true)))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => GetDeadline(src.DeadlineNum, src.DeadlineType)))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Author));

            CreateMap<AuthorService, AuthorServiceShowModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => GetImageSrc(src.Id, src.ImageSrc, src.ImageSrc_50, false)))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => GetDeadline(src.DeadlineNum, src.DeadlineType)))
                .ForMember(dest => dest.VkRepostCode, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.VkRepostConditions))
                        return null;
                    var repost = JsonConvert.DeserializeObject<VkRepostDTO>(src.VkRepostConditions);
                    return repost.ToVkCode();
                }));

            CreateMap<ServiceTypeModel, AuthorServiceType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<AuthorServiceType, ServiceTypeModel>();

            CreateMap<ServiceTopFilterModel, ServiceTopFilter>()
                .ForMember(dest => dest.MaxDate, opt => opt.MapFrom(src => src.InitialDate))
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.ServiceTypeId, opt => opt.MapFrom(src => src.ServiceType));
        }
        private string GetImageSrc(Guid id, string origin, string thumb, bool returnThumb)
        {
            if (Regex.IsMatch(origin, "https?://"))
                return returnThumb ? thumb : origin;
            else
                return $"{ImagesPath}{id}?resize={returnThumb}";
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
        
    }
}