using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Infrastructure.Binders
{
    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            return valueResult == null 
                ? base.BindModel(controllerContext, bindingContext) 
                : decimal.Parse(valueResult.AttemptedValue, CultureInfo.InvariantCulture);
        }
    }
}