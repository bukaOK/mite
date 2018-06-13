using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.CodeData.Constants.Automapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ProfileSettingsModel, User>()
                    .ForMember(dest => dest.UserName, opts => opts.MapFrom(src => src.NickName))
                    .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.About))
                    .ForMember(dest => dest.CityId, opts => opts.MapFrom(src => Guid.Parse(src.City)))
                    .ForMember(dest => dest.City, opts => opts.Ignore());
            CreateMap<User, ProfileSettingsModel>()
                .ForMember(dest => dest.NickName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.Description));

            CreateMap<User, ProfileModel>()
                .ForMember(dest => dest.PlaceName, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalLinks, opt => opt.Ignore())
                .ForMember(dest => dest.AvatarSrc, opt => opt.MapFrom(src => src.AvatarSrc ?? PathConstants.AvatarSrc))
                .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

            CreateMap<User, UserShortModel>()
                .ForMember(dest => dest.AvatarSrc, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.AvatarSrc))
                        return PathConstants.AvatarSrc;
                    var srcPath = HostingEnvironment.MapPath(src.AvatarSrc);
                    if (ImagesHelper.Compressed.CompressedExists(srcPath))
                        return ImagesHelper.Compressed.CompressedVirtualPath(srcPath);
                    return src.AvatarSrc;
                }))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var exist = context.Items.TryGetValue(UserProfileConstants.MaxAboutLength, out object maxAboutLength);
                    if (exist && src.Description != null && src.Description.Length > (int)maxAboutLength)
                        return src.Description.Substring(0, (int)maxAboutLength - 3) + "...";
                   return src.Description;
                }))
                .ForMember(dest => dest.UserName, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var exist = context.Items.TryGetValue(UserProfileConstants.MaxNameLength, out object maxNameLength);
                    if (exist && src.UserName.Length > (int)maxNameLength)
                        return src.UserName.Substring(0, (int)maxNameLength - 3) + "...";
                    return src.UserName;
                }));
        }
    }
}