using Mite.Attributes.DataAnnotations;
using Mite.CodeData.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Mite.Models
{
    public class CharacterModel
    {
        public Guid? Id { get; set; }
        [Required]
        [MaxLength(200)]
        [UIHint("TextBox")]
        [DisplayName("Имя персонажа")]
        [Placeholder("Джей Гэтсби")]
        public string Name { get; set; }
        [Required]
        [AllowHtml]
        public string Description { get; set; }
        [Required(ErrorMessage = "Выберите изображение")]
        public string ImageSrc { get; set; }
        /// <summary>
        /// Является ли персонаж оригинальным
        /// </summary>
        [UIHint("CheckBox")]
        [DisplayName("Оригинальный персонаж")]
        public bool Original { get; set; }
        public string UserId { get; set; }
        public string Universe { get; set; }
        public List<CharacterFeatureModel> Features { get; set; }
        public List<string> Universes { get; set; }
    }
    public class CharacterFeatureModel
    {
        public Guid? Id { get; set; }
        [Required]
        [MaxLength(100)]
        [UIHint("TextBox")]
        [DisplayName("Особенность")]
        [Placeholder("Рост")]
        public string FeatureName { get; set; }
        [Required]
        [MaxLength(300)]
        [DisplayName("Описание особенности")]
        [UIHint("TextArea")]
        [Placeholder("175")]
        public string FeatureDescription { get; set; }
    }
    public class UserCharactersModel
    {
        [DisplayName("Вид персонажа")]
        [UIHint("RadioButtonList")]
        [UIData(typeof(CharacterOriginalType))]
        public byte OriginalType { get; set; }
    }
}