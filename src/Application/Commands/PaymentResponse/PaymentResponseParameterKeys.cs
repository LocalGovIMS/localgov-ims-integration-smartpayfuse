namespace Application.Commands
{
    public static class PaymentResponseParameterKeys
    {
        public const string AuthorisationResult = "decision";
        public const string PspReference = "transaction_id";
        public const string MerchantReference = "req_reference_number";
        public const string MerchantSignature = "signature";
        public const string PaymentMethod = "card_type_name";
        public const string SigningField = "signingField";
    }
}
