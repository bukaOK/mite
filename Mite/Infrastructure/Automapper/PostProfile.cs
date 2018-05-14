using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using Mite.DAL.DTO;
using Mite.DAL.Entities;
using Mite.Models;
using NLog;
using System;
using System.Linq;
using System.Web.Hosting;

namespace Mite.Infrastructure.Automapper
{
    public class PostProfile : Profile
    {
        const string ImagesPath = "/images/post/";
        private readonly ILogger logger = LogManager.GetLogger("LOGGER");

        public PostProfile()
        {
            CreateMap<Post, PostModel>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Watermark, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentRating, opt => opt.Ignore());

            //Добавление поста юзером
            CreateMap<PostModel, Post>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Watermark, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.Type == PostTypes.Published ? (DateTime?)DateTime.UtcNow : null));
            //Пост галереи
            CreateMap<Post, GalleryPostModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => $"{ImagesPath}{src.Id}?watermark={src.WatermarkId != null}"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")));

            CreateMap<PostDTO, ProfilePostModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Length > 200
                    ? src.Description.Substring(0, 200) + "..." : src.Description))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.PublishDate != null))
                .ForMember(dest => dest.IsImage, opt => opt.ResolveUsing(src => src.ContentType != PostContentTypes.Document))
                .ForMember(dest => dest.ShowAdultContent, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var currentUser = (User)context.Items["currentUser"];
                    return (currentUser != null && currentUser.Age >= 18) || !src.Tags.Any(tag => tag.Name == "18+");
                }))
                .ForMember(dest => dest.IsGif, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document
                        ? ImagesHelper.IsAnimatedImage(HostingEnvironment.MapPath(src.Content)) : false))
                .ForMember(dest => dest.FullPath, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document 
                    ? $"{ImagesPath}{src.Id}?watermark={src.WatermarkId != null}" : null))
                .ForMember(dest => dest.Content, opt => opt.ResolveUsing((src, dest, value, context) =>
                {
                    var minChars = (int)context.Items["minChars"];
                    if (src == null)
                        return null;

                    switch (src.ContentType)
                    {
                        case PostContentTypes.Document:
                            try
                            {
                                return FilesHelper.ReadDocument(src.Content, minChars);
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Ошибка при чтении файла для работ пользователя, имя файла: {src.Content}, Ошибка : {e.Message}");
                                return "Ошибка при чтении файла.";
                            }
                        default:
                            return $"{ImagesPath}{src.Id}?resize=true&watermark={src.WatermarkId != null}";
                    }
                }))
                .ForMember(dest => dest.Cover, opt => opt.ResolveUsing((src, dest, value, context) =>
                {
                    if (src.ContentType != PostContentTypes.Document || src.Cover == null)
                        return null;
                    if (src.Cover_50 != null)
                        return src.Cover_50;
                    var fullImgPath = HostingEnvironment.MapPath(src.Cover);
                    return ImagesHelper.Compressed.CompressedExists(fullImgPath)
                        ? ImagesHelper.Compressed.CompressedVirtualPath(fullImgPath)
                        : src.Content;
                }));

            CreateMap<PostDTO, TopPostModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")))
                .ForMember(dest => dest.Cover, opt => opt.MapFrom(src => src.Cover_50 ?? src.Cover))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Length > 200 
                    ? src.Description.Substring(0, 200) + "..." : src.Description))
                .ForMember(dest => dest.IsImage, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document))
                .ForMember(dest => dest.Content, opt => opt.ResolveUsing((src, dest, value, context) =>
                {
                    var minChars = (int)context.Items["minChars"];
                    var currentUser = (User)context.Items["currentUser"];

                    switch (src.ContentType)
                    {
                        case PostContentTypes.Document:
                            try
                            {
                                return FilesHelper.ReadDocument(src.Content, minChars);
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Ошибка при чтении файла в топе, имя файла: {src.Content}, Ошибка : {e.Message}");
                                return "Ошибка при чтении файла.";
                            }
                        default:
                            return $"{ImagesPath}{src.Id}?resize=true&watermark={src.WatermarkId != null}";
                    }
                }))
                .ForMember(dest => dest.IsGif, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document
                        ? ImagesHelper.IsAnimatedImage(HostingEnvironment.MapPath(src.Content)) : false))
                .ForMember(dest => dest.FullPath, opt => opt.MapFrom(src => src.ContentType != PostContentTypes.Document 
                    ? $"{ImagesPath}{src.Id}?watermark={src.WatermarkId != null}" : null))
                .ForMember(dest => dest.ShowAdultContent, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var currentUser = (User)context.Items["currentUser"];
                    return (currentUser != null && currentUser.Age >= 18) || !src.Tags.Any(tag => tag.Name == "18+");
                }));

            CreateMap<PostModel, ImagePostModel>();
            CreateMap<PostModel, WritingPostModel>();

            CreateMap<PostCollectionItem, PostCollectionItemModel>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.ContentSrc));
            CreateMap<PostCollectionItemModel, PostCollectionItem>()
                .ForMember(dest => dest.ContentSrc_50, opt => opt.Ignore())
                .ForMember(dest => dest.ContentSrc, opt => opt.MapFrom(src => src.Content));
            CreateMap<ComicsItem, PostComicsItemModel>()
                .ForMember(dest => dest.CompressedContent, opt => opt.MapFrom(src => src.ContentSrc_50))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.ContentSrc));
            CreateMap<PostComicsItemModel, ComicsItem>()
                .ForMember(dest => dest.ContentSrc_50, opt => opt.Ignore())
                .ForMember(dest => dest.ContentSrc, opt => opt.MapFrom(src => src.Content));

            CreateMap<Post, GalleryItemModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => ImagesPath + src.Id + $"?watermark={src.WatermarkId != null}"))
                .ForMember(dest => dest.ImageCompressed, opt => opt.MapFrom(src => 
                    $"{ImagesPath}{src.Id}?watermark={src.WatermarkId != null}&resize=true"));
        }
    }
}