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
    public class UserTagDTO
    {
        /// <summary>
        /// Id тега
        /// </summary>
        public Guid Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Есть ли у текущего пользователя в списке
        /// </summary>
        public bool IsOwner { get; set; }
        public int Popularity { get; set; }
    }
}
