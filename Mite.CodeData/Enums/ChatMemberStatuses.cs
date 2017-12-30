using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum ChatMemberStatuses : byte
    {
        /// <summary>
        /// Актуален для всех сообщений
        /// </summary>
        InChat = 0,
        /// <summary>
        /// Исключен
        /// </summary>
        Excluded = 1,
        /// <summary>
        /// Вышел
        /// </summary>
        CameOut = 2,
        /// <summary>
        /// Удалил
        /// </summary>
        Removed = 3
    }
}
