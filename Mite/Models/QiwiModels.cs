using Mite.CodeData.Enums;
using Mite.Infrastructure.Binders;
using System.Web.Mvc;

namespace Mite.Models
{
    [ModelBinder(typeof(QiwiNotifyModelBinder))]
    public class QiwiNotifyModel
    {
        public string BillId { get; set; }
        public string Status { get; set; }
        public string UserPhone { get; set; }
        public string ProviderName { get; set; }
        public string Currency { get; set; }
        public string Comment { get; set; }
    }
}