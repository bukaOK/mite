using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

            CreateMap<User, ProfileModel>()
                    .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

            CreateMap<User, UserShortModel>();
        }
    }
}