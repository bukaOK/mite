using Dapper.Contrib.Extensions;
using Mite.DAL.Core;
using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Денежные операции между пользователями внутри системы
    /// (клик по рекламе, перечисление процента от вывода реферала и т.д.)
    /// </summary>
    public class CashOperation : GuidEntity
    {
        [ForeignKey("From")]
        public string FromId { get; set; }
        [ForeignKey("To")]
        public string ToId { get; set; }
        /// <summary>
        /// От кого
        /// </summary>
        public User From { get; set; }
        /// <summary>
        /// Кому
        /// </summary>
        public User To { get; set; }
        /// <summary>
        /// Сумма операции
        /// </summary>
        public double Sum { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
        public CashOperationTypes OperationType { get; set; }
    }
}