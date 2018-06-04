using Mite.Models;
using System.Web.Mvc;

namespace Mite.Infrastructure.Binders
{
    public class QiwiNotifyModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProvider = bindingContext.ValueProvider;
            var model = new QiwiNotifyModel
            {
                BillId = valueProvider.GetValue("bill_id").ConvertTo(typeof(string)) as string,
                Status = valueProvider.GetValue("status").ConvertTo(typeof(string)) as string,
                Comment = valueProvider.GetValue("status").ConvertTo(typeof(string)) as string,
                ProviderName = valueProvider.GetValue("prv_name").ConvertTo(typeof(string)) as string,
                UserPhone = valueProvider.GetValue("user").ConvertTo(typeof(string)) as string,
                Currency = valueProvider.GetValue("ccy").ConvertTo(typeof(string)) as string
            };
            return model;
        }
    }
}