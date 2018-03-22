using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;

namespace Mite.Infrastructure.Automapper
{
    public class FactProfile : Profile
    {
        public FactProfile()
        {
            CreateMap<DailyFact, DailyFactModel>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Used, opt => opt.MapFrom(src => src.EndDate != null || src.StartDate != null 
                    ? "Использован" : "Не использован"));
            CreateMap<DailyFactModel, DailyFact>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore());
        }
    }
}