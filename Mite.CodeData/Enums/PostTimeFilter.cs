using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    public enum PostTimeFilter
    {
        /// <summary>
        /// Посты за все время
        /// </summary>
        All,
        /// <summary>
        /// За этот месяц
        /// </summary>
        Month,
        /// <summary>
        /// Неделю
        /// </summary>
        Week,
        /// <summary>
        /// День
        /// </summary>
        Day
    }
}