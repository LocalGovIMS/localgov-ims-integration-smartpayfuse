
namespace Application.Commands
{
    public class PaymentRequestCommandResult
    {
        public string NextUrl { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public bool Finished { get; set; }
    }
}
