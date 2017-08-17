using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class City : GuidEntity
    {
        public string Name { get; set; }
        /// <summary>
        /// Регион
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// Федеральный округ
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// Часовой пояс
        /// </summary>
        public int? TimeZone { get; set; }
        /// <summary>
        /// Население
        /// </summary>
        public int Population { get; set; }
        /// <summary>
        /// Долгота
        /// </summary>
        public double? Longitude { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        public double? Latitude { get; set; }
    }
}