using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.DTO
{
    /// <summary>
    /// DTO для дешифровки сообщения
    /// </summary>
    public class DecryptMessageDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string IV { get; set; }
    }
}
