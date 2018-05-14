using AutoMapper;
using ImageMagick;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.Helpers;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class OrderProfile : Profile
    {
        const string ImagesPath = "/images/orders/";
        public OrderProfile()
        {
            CreateMap<OrderEditModel, Order>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.ImageSrc_600, opt => opt.Ignore());

            CreateMap<Order, OrderEditModel>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.OrderTypes, opt => opt.Ignore());

            CreateMap<Order, OrderShowModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => ImagesPath + src.Id))
                .ForMember(dest => dest.Deadline, opt => opt.ResolveUsing(src =>
                {
                    var deadlineCases = new string[3];
                    switch (src.DeadlineType)
                    {
                        case DurationTypes.Day:
                            deadlineCases = new[] { "дней", "день", "дня" };
                            break;
                        case DurationTypes.Hour:
                            deadlineCases = new[] { "часов", "час", "часа" };
                            break;
                        case DurationTypes.Month:
                            deadlineCases = new[] { "месяцев", "месяц", "месяца" };
                            break;
                        case DurationTypes.Week:
                            deadlineCases = new[] { "недель", "неделя", "недели" };
                            break;
                        default:
                            throw new NotImplementedException("Неизвестная продолжительность");
                    }

                    return $"{src.DeadlineNum} {ViewHelper.GetWordCase(src.DeadlineNum, deadlineCases[1], deadlineCases[2], deadlineCases[0])}";
                }))
                .ForMember(dest => dest.Executers, opt => opt.MapFrom(src => src.Requests.Select(x => x.Executer)));

            CreateMap<OrderTopDTO, OrderTopModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => ImagesPath + src.Id + "?resize=true"))
                .ForMember(dest => dest.Deadline, opt => opt.ResolveUsing(src =>
                {
                    var deadlineCases = new string[3];
                    switch (src.DeadlineType)
                    {
                        case DurationTypes.Day:
                            deadlineCases = new[] { "дней", "день", "дня" };
                            break;
                        case DurationTypes.Hour:
                            deadlineCases = new[] { "часов", "час", "часа" };
                            break;
                        case DurationTypes.Month:
                            deadlineCases = new[] { "месяцев", "месяц", "месяца" };
                            break;
                        case DurationTypes.Week:
                            deadlineCases = new[] { "недель", "неделя", "недели" };
                            break;
                        default:
                            throw new NotImplementedException("Неизвестная продолжительность");
                    }

                    return $"{src.DeadlineNum} {ViewHelper.GetWordCase(src.DeadlineNum, deadlineCases[1], deadlineCases[2], deadlineCases[0])}";
                }))
                .ForMember(dest => dest.Requests, opt => opt.MapFrom(src => src.RequestsCount == 0 ?
                    "Нет заявок" :
                    $"{src.RequestsCount} {ViewHelper.GetWordCase(src.RequestsCount, "заявка", "заявки", "заявок")}"));

            CreateMap<OrderTopFilterModel, OrderTopFilter>()
                .ForMember(dest => dest.MaxDate, opt => opt.MapFrom(src => src.InitialDate));
        }
    }
}