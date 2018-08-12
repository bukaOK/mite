using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;

namespace Mite.Infrastructure.Automapper
{
    public class TariffProfile : Profile
    {
        public TariffProfile()
        {
            CreateMap<TariffModel, AuthorTariff>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));
            CreateMap<AuthorTariff, TariffModel>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title));
        }
    }
}