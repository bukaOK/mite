using AutoMapper;
using Mite.BLL.Helpers;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.Models;
using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Mite.Infrastructure.Automapper
{
    public class CharacterProfile : Profile
    {
        const string ImagesPath = "/images/character/";
        Regex httpRegex = new Regex(@"^https?:\/\/", RegexOptions.Compiled);

        public CharacterProfile()
        {
            CreateMap<Character, CharacterModel>();
            CreateMap<CharacterModel, Character>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));

            CreateMap<Character, SelectListItem>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Universe) ? src.Name : $"{src.Name}({src.Universe})"))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<CharacterFeature, CharacterFeatureModel>()
                .ForMember(dest => dest.FeatureName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.FeatureDescription, opt => opt.MapFrom(src => src.Description));
            CreateMap<CharacterFeatureModel, CharacterFeature>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FeatureName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.FeatureDescription))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.NewGuid()));

            CreateMap<Character, ProfileCharacterModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => httpRegex.IsMatch(src.ImageSrc) ? src.ImageSrc : $"/images/resize?path={src.ImageSrc}&size=400"))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => FilesHelper.ReadDocument(src.DescriptionSrc, 230)));

            CreateMap<CharacterTopFilterModel, CharacterTopFilter>();
        }
    }
}