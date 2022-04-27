namespace Application.Commands
{
    public class RefundResult
    {
        public string PspReference { get; set; }
        public decimal? Amount { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }

        public static RefundResult Successful(string pspReference, decimal amount)
        {
            return new RefundResult()
            {
                Success = true,
                Amount = amount,
                PspReference = pspReference
            };
        }

        public static RefundResult Failure(string message)
        {
            return new RefundResult()
            {
                Success = false,
                Message = message
            };
        }
    }
}
