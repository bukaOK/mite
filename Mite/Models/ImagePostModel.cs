using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Mite.Attributes.DataAnnotations;
using Mite.Enums;

namespace Mite.Models
{
    public class ImagePostModel
    {
        public Guid Id { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Заголовок")]
        [Required]
        public string Header { get; set; }
        [Required(ErrorMessage = "Вы не загрузили изображение")]
        public string Content { get; set; }
        [DisplayName("Описание")]
        [MaxLength(200, ErrorMessage = "Слишком большое описание")]
        [UIHint("TextArea")]
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
}