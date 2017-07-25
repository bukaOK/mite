using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    public class AuthorServiceType : Entity
    {
        /// <summary>
        /// Название типа услуги(портрерт, фотосессия и пр.)
        /// </summary>
        public string Name { get; set; }
    }
}