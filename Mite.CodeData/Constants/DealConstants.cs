using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Constants
{
    public static class DealConstants
    {
        /// <summary>
        /// Коэффициент, на который умножаются успешные сделки
        /// </summary>
        public const int GoodCoef = 1;
        /// <summary>
        /// Коэффициент, на который умножаются сделки с участием модератора
        /// </summary>
        public const int BadCoef = -3;
    }
}
