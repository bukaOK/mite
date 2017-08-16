using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Услуги, которые могут предоставлять авторы
    /// </summary>
    public class AuthorService : GuidEntity
    {
        public string Name { get; set; }
        public IEnumerable<User> Authors { get; set; }
    }
}