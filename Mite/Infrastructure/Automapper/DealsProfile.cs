using AutoMapper;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
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
                    ? ViewHelper.GetPastTense(DateTime.UtcNow, src.Deadline.Value, "", "срок истек", true)
                    : "дата ожидается"))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Service != null ? src.Service.Title : src.Order.Title))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src =>
                {
                    if (src.Service != null)
                        return src.Service.Description;
                    var descr = src.Order.Description;
                    return descr.Length > 400 ? descr.Substring(0, 400) + "..." : descr;
                }))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.Service != null ? src.Service.ImageSrc_50 : src.Order.ImageSrc_600))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price != null ? $"{src.Price} руб" : "цена ожидается"))
                .ForMember(dest => dest.New, opt => opt.MapFrom(src => src.Price == null || src.Deadline == null))
                .ForMember(dest => dest.ForModer, opt => opt.ResolveUsing((src, dest, member, context) =>
                {
                    if (context.Items.TryGetValue("forModer", out object forModer) && (bool)forModer)
                        return true;
                    return false;
                }));

            CreateMap<Deal, DealModel>()
                .ForMember(dest => dest.DeadlineStr, opt => opt.MapFrom(src =>
                    src.Deadline.HasValue ? src.Deadline.Value.ToString("dd.MM.yyyy") : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.ImageResultSrc));

            CreateMap<DealModel, DealClientModel>();
            CreateMap<DealModel, DealAuthorModel>();

            CreateMap<DealGalleryItemDTO, GalleryItemModel>();
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