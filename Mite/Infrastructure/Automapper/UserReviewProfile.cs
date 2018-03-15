using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;

namespace Mite.Infrastructure.Automapper
{
    public class UserReviewProfile : Profile
    {
        public UserReviewProfile()
        {
            CreateMap<UserReview, AdminUserReviewModel>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        }
    }
}