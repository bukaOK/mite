using Mite.DAL.Core;
using Mite.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Mite.DAL.Entities
{
    /// <summary>
    /// Представляет собой сущность операции ввода-вывода денег
    /// </summary>
    public class Payment : GuidEntity
    {
        /// <summary>
        /// Сумма(может быть отрицательной, если пользователь вывел деньги)
        /// </summary>
        public double Sum { get; set; }
        /// <summary>
        /// Дата платежа
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Id операции стороннего сервиса(например яндекса)
        /// </summary>
        public string OperationId { get; set; }
        /// <summary>
        /// Тип операции
        /// </summary>
        public PaymentType PaymentType { get; set; }
        /// <summary>
        /// Пользователь, с которым совершалась операция
        /// </summary>
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}