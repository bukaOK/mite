using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Mite.DAL.Entities;
using Mite.DAL.DTO;
using Mite.Models;

namespace Mite.Infrastructure.Automapper
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<string, Tag>()
                    .ConvertUsing(x => new Tag
                    {
                        Name = x.ToLower()
                    });
            CreateMap<Tag, string>()
                .ConvertUsing(x => x.Name);
            CreateMap<TagDTO, TagModel>();
        }
    }
}