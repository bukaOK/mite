using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mite.Attributes.DataAnnotations;
using Mite.Enums;

namespace Mite.Models
{
    public class WritingPostModel
    {
        public Guid Id { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Заголовок")]
        [Required]
        public string Header { get; set; }
        [Required(ErrorMessage = "Вы не заполнили контент")]
        public string Content { get; set; }
        [UIHint("TextArea")]
        [MaxLength(200, ErrorMessage = "Слишком большое описание")]
        [DisplayName("Описание")]
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
}