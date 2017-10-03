using Mite.ExternalServices.WebMoney.Business;
using Mite.ExternalServices.WebMoney.Business.Enums;

namespace Mite.ExternalServices.WebMoney.Params
{
    public class ExpressPaymentParams
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public Phone ClientPhone { get; set; }
        public ExpressConfirmTypes ConfirmType { get; set; }
        public ExpressClientLoginTypes LoginType { get; set; }
        public bool EmulatedFlag { get; set; }
    }
}
