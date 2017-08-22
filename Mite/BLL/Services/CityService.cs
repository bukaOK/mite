using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;
using NLog;
using Mite.Models;
using AutoMapper;

namespace Mite.BLL.Services
{
    public interface ICityService : ICrudService<CityModel, CitiesRepository, City>
    {
        //Task<DataServiceResult> AddCityToUser();
    }
    public class CityService : CrudService<CityModel, CitiesRepository, City>, ICityService
    {
        public CityService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {

        }
    }
}
