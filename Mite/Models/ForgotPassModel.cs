using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class ForgotPassModel
    {
        [Required]
        [DisplayName("E-mail")]
        [UIHint("TextBox")]
        [EmailAddress]
        [DataType("text")]
        public string Email { get; set; }
    }
}