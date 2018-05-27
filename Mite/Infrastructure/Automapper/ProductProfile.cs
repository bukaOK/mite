using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.Models;
using System;
using System.Linq;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class ProductProfile : Profile
    {
        const string ImageCachePath = "/images/post/";
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.BonusBase64, opt => opt.MapFrom(src => src.BonusPath));
            CreateMap<ProductModel, Product>()
                .ForMember(dest => dest.BonusPath, opt => opt.Ignore());

            CreateMap<PostDTO, ProductDTO>()
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.Id));

            CreateMap<ProductItemModel, ProductCollectionItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));
            CreateMap<ProductCollectionItem, ProductItemModel>();

            CreateMap<ProductDTO, ProductTopModel>()
                .ForMember(dest => dest.Cover, opt => opt.MapFrom(src => src.Cover_50 ?? src.Cover))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Length > 200
                    ? src.Description.Substring(0, 200) + "..." : src.Description))
                .ForMember(dest => dest.IsImage, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document))
                .ForMember(dest => dest.IsGif, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document
                        ? ImagesHelper.IsAnimatedImage(HostingEnvironment.MapPath(src.Content)) : false))
                .ForMember(dest => dest.FullPath, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document
                    ? $"{ImageCachePath}{src.Id}?watermark={src.WatermarkId != null}" : null))
                .ForMember(dest => dest.ShowAdultContent, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var currentUser = (User)context.Items["currentUser"];
                    return (currentUser != null && currentUser.Age >= 18) || !src.Tags.Any(tag => tag.Name == "18+");
                }))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src =>
                    $"{ImageCachePath}{src.PostId}?watermark={src.WatermarkId != null}&resize=true"))
                .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId.ToString("N")))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")));
            
            CreateMap<ProductTopFilterModel, ProductTopFilter>()
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.MaxDate, opt => opt.MapFrom(src => src.InitialDate));
        }
    }
}