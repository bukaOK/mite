using Autofac;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace Mite
{
    public static class CitiesInitializer
    {
        private static string CitiesCsvPath => HostingEnvironment.MapPath("~/Files/Cities.csv");
        public static void Initialize()
        {
            IUnitOfWork unitOfWork = new UnitOfWork();
            var citiesRepo = unitOfWork.GetRepo<CitiesRepository, City>();
            if (citiesRepo.CitiesInitialized)
                return;

            var reader = new StreamReader(CitiesCsvPath, Encoding.Default);
            var cities = ReadCities(reader);
            reader.Close();

            citiesRepo.AddRange(cities);
        }
        private static IList<City> ReadCities(StreamReader reader)
        {
            var rows = reader.ReadToEnd().Split('\n');
            var cities = new List<City>();
            for (var i = 2; i < rows.Length; i++)
            {
                var columns = rows[i].Split(';');
                int population;
                int.TryParse(columns[4], out population);

                cities.Add(new City
                {
                    Name = columns[1],
                    Region = columns[2],
                    District = columns[3],
                    Population = population
                });
            }
            return cities;
        }
    }
}