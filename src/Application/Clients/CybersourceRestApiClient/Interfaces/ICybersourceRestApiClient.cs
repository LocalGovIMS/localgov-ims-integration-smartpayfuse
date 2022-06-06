using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Entities;

namespace Application.Clients.CybersourceRestApiClient.Interfaces
{
    public interface ICybersourceRestApiClient
    {
        Task<bool> RefundPayment(string clientReference, string pspReference, decimal amount);
        Task<List<Payment>> SearchPayments(string clientReference, int daysAgo);

        Task<List<Payment>> SearchRefunds(string clientReference, int daysAgo);

    }
}