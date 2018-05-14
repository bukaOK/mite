using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;

namespace Mite.Infrastructure.Automapper
{
    public class WatermarkProfile : Profile
    {
        public WatermarkProfile()
        {
            CreateMap<WatermarkEditModel, Watermark>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()))
                .ForMember(dest => dest.Invert, opt => opt.MapFrom(src => src.Inverted))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.WmText));
        }
    }
}