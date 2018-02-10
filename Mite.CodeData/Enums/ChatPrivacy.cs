using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    /// <summary>
    /// Кто может писать текущему пользователю
    /// </summary>
    public enum ChatPrivacy : byte
    {
        /// <summary>
        /// Все
        /// </summary>
        All = 0,
        /// <summary>
        /// Подписчики
        /// </summary>
        Followers = 1,
        /// <summary>
        /// Только те, на кого подписан пользователь
        /// </summary>
        Followings = 2,
        /// <summary>
        /// Подписчики и те, на кого подписан пользователь
        /// </summary>
        FollowersAndFollowings = 3
    }
}
