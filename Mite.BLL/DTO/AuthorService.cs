using Mite.DAL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.DTO
{
    /// <summary>
    /// Услуги, которые могут предоставлять авторы
    /// </summary>
    public class AuthorServiceDTO
    {
        public string Name { get; set; }
        public IEnumerable<UserDTO> Authors { get; set; }
    }
}