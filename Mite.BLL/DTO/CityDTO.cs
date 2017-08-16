using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.DTO
{
    public class CityDTO
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
    }
}