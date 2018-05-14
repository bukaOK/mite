using Mite.CodeData.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Mite.Models
{
    public class WatermarkShortModel
    {
        public string WmPath { get; set; }
        public ImageGravity Gravity { get; set; }
    }
    public class WatermarkEditModel
    {
        [DisplayName("Расположение")]
        public ImageGravity Gravity { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Текст")]
        public string WmText { get; set; }
        [UIHint("TextBox")]
        [DisplayName("Размер шрифта")]
        public int? FontSize { get; set; }
        [UIHint("Checkbox")]
        [DisplayName("Инвертировать")]
        public bool Inverted { get; set; }
        public Guid? Id { get; set; }
        public string WmPath { get; set; }
        public bool AddToWmCollection { get; set; }
        /// <summary>
        /// Выбирается в качестве водяного знака изображение, загруженное пользователем
        /// </summary>
        public bool UseCustomImage { get; set; }
    }
}