using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData
{
    public enum RegisterRoles : byte
    {
        [Display(Name = "Автор")]
        Author = 0,
        [Display(Name = "Клиент")]
        Client = 1
    }
}
