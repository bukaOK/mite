using Mite.Attributes.DataAnnotations;
using Mite.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class PayOutModel
    {
        [Required]
        [UIHint("TextBox")]
        [DataType("number")]
        [Display(Name = "Сумма")]
        public int? PayOutSum { get; set; }
    }
    public class PayInModel
    {
        [Required]
        [UIHint("TextBox")]
        [DataType("number")]
        [Display(Name = "Сумма")]
        public int? PayInSum { get; set; }
        [Required]
        [UIHint("DropDownList")]
        [Display(Name = "Платежная система")]
        [UIData(typeof(PaymentType))]
        public int PaymentType { get; set; }
    }
}