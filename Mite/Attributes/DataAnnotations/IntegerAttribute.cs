using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Attributes.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IntegerAttribute : DataTypeAttribute
    {
        public IntegerAttribute() : base("integer")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage == null && ErrorMessageResourceName == null)
            {
                ErrorMessage = "Введите целочисленное значение в поле " + name; 
            }
            return base.FormatErrorMessage(name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;
            return int.TryParse(Convert.ToString(value), out int retNum);
        }
    }
}