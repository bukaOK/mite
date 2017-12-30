using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.CodeData.Constants
{
    public static class SessionKeys
    {
        /// <summary>
        /// Используется при платеже с банковской карты
        /// </summary>
        public const string YaMoneyExternal = "YaMoneyExternalPayment";
        /// <summary>
        /// Экспресс платеж WebMoney хранит InvoiceId(uint)
        /// </summary>
        public const string WebMoneyExpressInvoiceId = "WebMoneyExpressInvoiceId";
        /// <summary>
        /// Ключ к списку новых чатов(они ещё не в базе и нет сообщений)
        /// </summary>
        public const string NewChats = "NewChats";
    }
}