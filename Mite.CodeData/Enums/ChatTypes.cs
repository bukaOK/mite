using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.CodeData.Enums
{
    public enum ChatTypes : byte
    {
        /// <summary>
        /// Скрытый чат(диалоги)
        /// </summary>
        Private = 0,
        /// <summary>
        /// Открытый чат с несколькими участниками
        /// </summary>
        Public = 1,
        /// <summary>
        /// Чат спора в сделке
        /// </summary>
        Dispute = 2,
        /// <summary>
        /// Скрытый с несколькими участниками
        /// </summary>
        PrivateGroup = 3,
        /// <summary>
        /// Чат сделки
        /// </summary>
        Deal = 4
    }
}
