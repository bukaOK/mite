using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using Mite.Attributes.DataAnnotations;

namespace Mite.Models
{
    public class LoginModel
    {
        [Required]
        [UIHint("TextBox")]
        [DisplayName("Ник")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        [UIHint("TextBox")]
        public string Password { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Запомнить меня")]
        public bool Remember { get; set; }
    }
}