using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum DialogTypes : byte
    {
        /// <summary>
        /// Диалог м/у двумя
        /// </summary>
        Dialog = 0,
        /// <summary>
        /// Комната(много человек)
        /// </summary>
        Room = 1
    }
}
