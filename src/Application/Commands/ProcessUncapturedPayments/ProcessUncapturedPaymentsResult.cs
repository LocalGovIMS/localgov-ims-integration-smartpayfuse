namespace Application.Commands
{
    public class ProcessUncapturedPaymentsResult
    {
        public int TotalIdentified { get; set; }
        public int TotalMarkedAsCaptured { get; set; }
        public int TotalErrors { get; set; }

    }
}
