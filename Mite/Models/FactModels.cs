using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mite.Models
{
    public class DailyFactModel
    {
        public Guid? Id { get; set; }
        [Required]
        [MaxLength(100)]
        [UIHint("TextBox")]
        [DisplayName("Название")]
        public string Header { get; set; }
        [Required]
        [MaxLength(400)]
        [UIHint("TextArea")]
        [DisplayName("Название")]
        public string Content { get; set; }
        public string Used { get; set; }
    }
}