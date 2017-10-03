using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<Rating, CommentRatingModel>();
            CreateMap<Rating, PostRatingModel>();

            CreateMap<CommentRatingModel, Rating>();
            CreateMap<PostRatingModel, Rating>();
            CreateMap<Rating, CommentRatingModel>();
        }
    }
}