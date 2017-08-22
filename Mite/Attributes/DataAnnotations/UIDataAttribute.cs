using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Mite.Attributes.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UIDataAttribute : Attribute
    {
        public IList<SelectListItem> SelectListItems { get; }
        
        public UIDataAttribute(Type instanceType)
        {
            if(!EnumHelper.IsValidForEnumHelper(instanceType))
                throw new ArgumentException("Тип должен быть enum");
            SelectListItems = EnumHelper.GetSelectList(instanceType).ToList();
        }
    }
}