using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    public class TagDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsConfirmed { get; set; }
        public bool Checked { get; set; }
        public int Popularity { get; set; }
    }
}
