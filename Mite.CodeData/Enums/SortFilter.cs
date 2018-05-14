using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Фильтр, какие записи, комменты и тд идут первыми
    /// </summary>
    public enum SortFilter
    {
        /// <summary>
        /// Популярные
        /// </summary>
        Popular = 0,
        /// <summary>
        /// Новые
        /// </summary>
        New = 1,
        /// <summary>
        /// Старые
        /// </summary>
        Old = 2,
        /// <summary>
        /// Надежные
        /// </summary>
        Reliable = 3,
        Expensive = 4,
        Cheap = 5
    }
}