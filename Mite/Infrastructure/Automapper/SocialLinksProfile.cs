using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class SocialLinksProfile : Profile
    {
        public SocialLinksProfile()
        {
            CreateMap<SocialLinks, SocialLinksModel>();
            CreateMap<SocialLinksModel, SocialLinks>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}