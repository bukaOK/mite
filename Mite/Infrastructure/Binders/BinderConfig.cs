using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mite.Infrastructure.Binders
{
    public static class BinderConfig
    {
        public static void RegisterBinders()
        {
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(double?), new DoubleModelBinder());
            ModelBinders.Binders.Add(typeof(double), new DoubleModelBinder());
        }
    }
}