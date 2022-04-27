namespace Application.Commands
{
    public static class PaymentResponseParameterKeys
    {
        public const string AuthorisationResult = "authResult";
        public const string PspReference = "pspReference";
        public const string MerchantReference = "merchantReference";
        public const string MerchantSignature = "merchantSig";
        public const string PaymentMethod = "paymentMethod";
    }
}
