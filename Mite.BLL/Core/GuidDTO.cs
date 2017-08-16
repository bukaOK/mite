using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.BLL.Core
{
    public interface IGuidDTO
    {
        Guid Id { get; set; }
    }
    public class GuidDTO : IGuidDTO
    {
        public Guid Id { get; set; }
    }
}
