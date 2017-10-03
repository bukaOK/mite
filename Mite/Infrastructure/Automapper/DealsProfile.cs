using AutoMapper;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.Helpers;
using Mite.Models;
using System;

namespace Mite.Infrastructure.Automapper
{
    public class DealsProfile : Profile
    {
        public DealsProfile()
        {
            CreateMap<Deal, DealUserModel>()
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Deadline != null 
                    ? ViewHelper.GetPastTense(DateTime.UtcNow, src.Deadline.Value, "осталось", "просрочено")
                    : "дата ожидается"))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Service.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.Service.ImageSrc_50))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price != null ? $"{src.Price} руб" : "цена ожидается"))
                .ForMember(dest => dest.New, opt => opt.MapFrom(src => src.Price == null || src.Deadline == null));

            CreateMap<Deal, DealModel>()
                .ForMember(dest => dest.DeadlineStr, opt => opt.MapFrom(src =>
                    src.Deadline.HasValue ? src.Deadline.Value.ToString("dd.MM.yyyy") : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.ImageResultSrc));

            CreateMap<DealModel, DealClientModel>();
            CreateMap<DealModel, DealAuthorModel>();
        }
        private DateTime GetEndDate(int deadlineNum, DurationTypes durationType)
        {
            var date = DateTime.UtcNow;
            switch (durationType)
            {
                case DurationTypes.Day:
                    date.AddDays(deadlineNum);
                    break;
                case DurationTypes.Hour:
                    date.AddHours(deadlineNum);
                    break;
                case DurationTypes.Month:
                    date.AddMonths(deadlineNum);
                    break;
                case DurationTypes.Week:
                    date.AddDays(7 * deadlineNum);
                    break;
                default:
                    throw new NotImplementedException("Неизвестный тип продолжительности");
            }
            return date;
        }
    }
}