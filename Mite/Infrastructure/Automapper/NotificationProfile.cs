using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<NotificationModel, Notification>()
                    .ForMember(dest => dest.NotifyUserId, opt => opt.MapFrom(src => src.NotifyUser.Id))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                    .ForMember(dest => dest.NotifyUser, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());
            CreateMap<Notification, NotificationModel>();
        }
    }
}