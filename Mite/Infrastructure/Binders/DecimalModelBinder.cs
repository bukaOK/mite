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

            if (decimal.TryParse(valueResult.AttemptedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                return result;
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}