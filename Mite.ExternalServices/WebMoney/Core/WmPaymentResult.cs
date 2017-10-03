namespace Mite.ExternalServices.WebMoney.Core
{
    public class WmPaymentResult
    {
        public bool Succeeded { get; }
        public object ResultData { get; }
        public string ErrorMessage { get; }

        private WmPaymentResult(bool succeeded, string message)
        {
            Succeeded = succeeded;
            ErrorMessage = message;
        }
        private WmPaymentResult(bool succeeded, object data)
        {
            Succeeded = succeeded;
            ResultData = data;
        }
        private WmPaymentResult(bool succeeded, string message, object data)
        {
            Succeeded = succeeded;
            ErrorMessage = message;
            ResultData = data;
        }
        public static WmPaymentResult Failed(string message)
        {
            return new WmPaymentResult(false, message);
        }
        public static WmPaymentResult Success(object data)
        {
            return new WmPaymentResult(true, data);
        }
        public static WmPaymentResult Success()
        {
            return new WmPaymentResult(true, null);
        }
    }
}
