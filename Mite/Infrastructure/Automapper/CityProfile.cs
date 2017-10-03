using AutoMapper;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<CityModel, City>();
            CreateMap<City, CityModel>();
        }
    }
}