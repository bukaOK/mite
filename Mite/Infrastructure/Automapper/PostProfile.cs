using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, PostModel>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.CurrentRating, opt => opt.Ignore());
                //Если это изображение и Cover не Null, значит перед нами коллекция изображений, и надо менять контент и Cover местами
                //все для удобства
                //.ForMember(dest => dest.Content, opt => opt.MapFrom(src =>
                //    src.IsImage && !string.IsNullOrEmpty(src.Cover) ? src.Cover : src.Content))
                //.ForMember(dest => dest.Cover, opt => opt.MapFrom(src =>
                //    src.IsImage && !string.IsNullOrEmpty(src.Cover) ? src.Content : src.Cover));

            //Добавление поста юзером
            CreateMap<PostModel, Post>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.Type == PostTypes.Published ? (DateTime?)DateTime.UtcNow : null));
            //Пост галереи
            CreateMap<Post, GalleryPostModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")));

            CreateMap<Post, ProfilePostModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.PublishDate != null))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content_50 ?? src.Content))
                .ForMember(dest => dest.Cover, opt => opt.MapFrom(src => src.Cover_50 ?? src.Cover));
            CreateMap<Post, TopPostModel>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content_50 ?? src.Content))
                .ForMember(dest => dest.Cover, opt => opt.MapFrom(src => src.Cover_50 ?? src.Cover));

            CreateMap<PostModel, ImagePostModel>();
            CreateMap<PostModel, WritingPostModel>();
            CreateMap<PostModel, ImageCollectionPostModel>();
            CreateMap<Post, GalleryItemModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.Content.Replace('\\', '/')))
                .ForMember(dest => dest.ImageCompressed, opt => opt.ResolveUsing(src =>
                {
                    if (string.IsNullOrEmpty(src.Content_50))
                    {
                        src.Content_50 = ImagesHelper.Compressed.CompressedVirtualPath(HostingEnvironment.MapPath(src.Content));
                    }
                    return src.Content_50.Replace('\\', '/');
                }));
        }
    }
}