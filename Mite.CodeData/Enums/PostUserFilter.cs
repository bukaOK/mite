using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Enums
{
    public enum PostUserFilter
    {
        /// <summary>
        /// Посты всех
        /// </summary>
        All,
        /// <summary>
        /// Только тех, на кого пользователь подписан
        /// </summary>
        OnlyFollowings
    }
}