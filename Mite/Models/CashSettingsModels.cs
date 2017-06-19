using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class CashModel
    {
        public double CashSum { get; set; }
        public bool IsYandexAuthorized { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string SystemYandexWallet { get; set; }
    }
    public class ReferalModel
    {
        /// <summary>
        /// Ник реферала
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Сколько всего заработал данный реферал для пользователя
        /// </summary>
        public double Income { get; set; }
    }
    public class OperationModel
    {
        /// <summary>
        /// Сумма платежа(может быть и отрицательной, если пользователь вывел деньги, или отправил другому пользователю)
        /// </summary>
        public int Sum { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Как заплатил(банк. карта, яндекс кошелек и т.д.)
        /// </summary>
        public string Type { get; set; }
    }
}