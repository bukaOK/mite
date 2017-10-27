using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Attributes.Validation
{
    /// <summary>
    /// Атрибут валидации для checkbox и radiobutton(работает аналогично required). 
    /// Больше необходим для клиентской валидации
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CheckedAttribute : ValidationAttribute
    {
        public override bool RequiresValidationContext => true;

        public CheckedAttribute() : base()
        {
        }
        public override bool IsValid(object value)
        {
            return value != null;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult($"Выберите {validationContext.DisplayName}");
            }
            return ValidationResult.Success;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"Выберите {name}";
        }
    }
}