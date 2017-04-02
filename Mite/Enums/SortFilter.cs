using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Enums
{
    /// <summary>
    /// Фильтр, какие записи, комменты и тд идут первыми
    /// </summary>
    public enum SortFilter
    {
        /// <summary>
        /// Популярные
        /// </summary>
        Popular,
        /// <summary>
        /// Новые
        /// </summary>
        New,
        /// <summary>
        /// Старые
        /// </summary>
        Old
    }
}