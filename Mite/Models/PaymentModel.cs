using Mite.Attributes.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    /// <summary>
    /// Вывод через яндекс.деньги
    /// </summary>
    public class YaPayOutModel
    {
        [Required]
        [UIHint("TextBox")]
        [Display(Name = "Сумма")]
        public int? PayOutSum { get; set; }
    }
    /// <summary>
    /// Ввод через яндекс.деньги
    /// </summary>
    public class YaPayInModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Сумма")]
        public int? YaPayInSum { get; set; }
    }
    /// <summary>
    /// Платеж через банковскую карту
    /// </summary>
    public class BankPayInModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Сумма")]
        public int? BankPayInSum { get; set; }
    }
    /// <summary>
    /// Ввод через webmoney
    /// </summary>
    public class WmPayInModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Сумма")]
        public int? WmPayInSum { get; set; }
        [Required, OffClientValidation]
        [UIHint("TextBox")]
        [DisplayName("Телефон")]
        [RegularExpression(@"\+7\([0-9]{3}\)[0-9]{3}-[0-9]{2}-[0-9]{2}", ErrorMessage = "Введите правильный номер")]
        public string WmPhoneNumber { get; set; }
    }
    /// <summary>
    /// Ввод через Qiwi
    /// </summary>
    public class QiwiPayInModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Сумма")]
        public int? QiwiPayInSum { get; set; }
        [Required, OffClientValidation]
        [UIHint("TextBox")]
        [DisplayName("Телефон")]
        [RegularExpression(@"\+7\([0-9]{3}\)[0-9]{3}-[0-9]{2}-[0-9]{2}", ErrorMessage = "Введите правильный номер")]
        public string QiwiPhoneNumber { get; set; }
    }
    public class WmPayInConfirmModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Код подтверждения")]
        public string WmConfirmCode { get; set; }
    }
}