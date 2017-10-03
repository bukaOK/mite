using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    public class AuthorServiceTypeDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Confirmed { get; set; }
        public int Popularity { get; set; }
    }
}
