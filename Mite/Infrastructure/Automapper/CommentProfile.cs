using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CommentModel>()
                    .ForMember(dest => dest.CurrentRating, opt => opt.Ignore());
            CreateMap<CommentModel, Comment>()
                .ForMember(dest => dest.Ratings, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.ParentCommentId,
                    opt => opt.MapFrom(src => src.ParentComment == null ? null : (Guid?)src.ParentComment.Id));
        }
    }
}