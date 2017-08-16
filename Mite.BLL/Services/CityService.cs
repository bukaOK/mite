using Mite.BLL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mite.DAL.Infrastructure;

namespace Mite.BLL.Services
{
    public interface ICityService : ICrudService<City, CitiesRepository, City>
    {
    }
    public class CityService : CrudService<City, CitiesRepository, City>
    {
        public CityService(IUnitOfWork database) : base(database)
        {
        }

        public override Task<DataServiceResult> UpdateAsync(City model)
        {
            throw new NotImplementedException();
        }
    }
}
