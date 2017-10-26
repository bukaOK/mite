using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum ServiceSortFilter : byte
    {
        /// <summary>
        /// Популярный(по рейтингу)
        /// </summary>
        Popular = 0,
        /// <summary>
        /// Надежный(по надежности услуги)
        /// </summary>
        Reliable = 1
    }
}
