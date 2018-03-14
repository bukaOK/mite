using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Infrastructure.Binders
{
    public class DoubleModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (double.TryParse(valueResult.AttemptedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double result))
                return result;
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}