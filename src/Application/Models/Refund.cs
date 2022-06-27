using System;
using System.Diagnostics.CodeAnalysis;

namespace Application.Models
{
    [ExcludeFromCodeCoverage]
    public class Refund
    {
        public string Reference { get; set; }
        public string ImsReference { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
