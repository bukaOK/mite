﻿using AutoMapper;
using Mite.BLL.DTO;
using Mite.DAL.Entities;
using Mite.Models;
using NLog;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Mite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILogger logger = LogManager.GetLogger("LOGGER");

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureAutoMapper();
            //Mapper.Configuration.AssertConfigurationIsValid();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            logger.Error(exception, "Unhandled Error");
        }
        private void ConfigureAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<string, Tag>()
                    .ConvertUsing(x => new Tag
                    {
                        Name = x.ToLower()
                    });
                cfg.CreateMap<Tag, string>()
                    .ConvertUsing(x => x.Name);
                cfg.CreateMap<TagDTO, TagModel>();

                cfg.CreateMap<User, UserShortModel>();

                cfg.CreateMap<Comment, CommentModel>()
                    .ForMember(dest => dest.CurrentRating, opt => opt.Ignore());
                cfg.CreateMap<CommentModel, Comment>()
                    .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore())
                    .ForMember(dest => dest.Post, opt => opt.Ignore())
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                    .ForMember(dest => dest.ParentCommentId, 
                        opt => opt.MapFrom(src => src.ParentComment == null ? null : (Guid?)src.ParentComment.Id));

                cfg.CreateMap<Rating, CommentRatingModel>();
                cfg.CreateMap<Rating, PostRatingModel>();

                cfg.CreateMap<Post, PostModel>()
                    .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                    .ForMember(dest => dest.CurrentRating, opt => opt.Ignore())
                    .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.PublishDate != null));

                //Добавление поста юзером
                cfg.CreateMap<PostModel, Post>()
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Header))
                    .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore())
                    .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.IsPublished ? (DateTime?)DateTime.UtcNow : null));
                //Пост галереи
                cfg.CreateMap<Post, GalleryPostModel>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")));

                cfg.CreateMap<ProfileSettingsModel, User>()
                    .ForMember(dest => dest.UserName, opts => opts.MapFrom(src => src.NickName))
                    .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.About));

                cfg.CreateMap<Post, ProfilePostModel>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("N")))
                    .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.Title))
                    .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.PublishDate != null));
                cfg.CreateMap<Post, TopPostModel>();

                cfg.CreateMap<User, ProfileModel>()
                    .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

                cfg.CreateMap<CommentRatingModel, Rating>();
                cfg.CreateMap<PostRatingModel, Rating>();
                cfg.CreateMap<Rating, CommentRatingModel>();

                cfg.CreateMap<PostModel, ImagePostModel>();
                cfg.CreateMap<PostModel, WritingPostModel>();

                cfg.CreateMap<NotificationModel, Notification>()
                    .ForMember(dest => dest.NotifyUserId, opt => opt.MapFrom(src => src.NotifyUser.Id))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                    .ForMember(dest => dest.NotifyUser, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());
                cfg.CreateMap<Notification, NotificationModel>();
                cfg.CreateMap<Helper, HelperModel>();

                cfg.CreateMap<Payment, OperationModel>()
                    .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src =>
                    {
                        switch (src.PaymentType)
                        {
                            case Enums.PaymentType.BankCard:
                                return "Банковская карта";
                            case Enums.PaymentType.YandexWallet:
                                return "Яндекс.Деньги";
                            default:
                                throw new ArgumentException("Неизвестный тип платежа");
                        }
                    }));

                cfg.CreateMap<SocialLinks, SocialLinksModel>();
                cfg.CreateMap<SocialLinksModel, SocialLinks>()
                    .ForMember(dest => dest.UserId, opt => opt.Ignore());
            });
        }
    }
}
