using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Attributes.DataAnnotations
{
    /// <summary>
    /// На свойстве с этим атрибутом не нужна клиентская валидация
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OffClientValidationAttribute : Attribute
    {
    }
}