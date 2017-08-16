using Mite.CodeData.Enums;
using System;

namespace Mite.BLL.DTO
{
    /// <summary>
    /// Денежные операции между пользователями внутри системы
    /// (клик по рекламе, перечисление процента от вывода реферала и т.д.)
    /// </summary>
    public class CashOperationDTO
    {
        public Guid Id { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        /// <summary>
        /// От кого
        /// </summary>
        public UserDTO From { get; set; }
        /// <summary>
        /// Кому
        /// </summary>
        public UserDTO To { get; set; }
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