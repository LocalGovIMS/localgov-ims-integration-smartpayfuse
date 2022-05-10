using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Clients.LocalGovImsPaymentApi;

namespace Application.Clients.CybersourceRestApiClient.Interfaces
{
    public interface ICybersourceRestApiClient
    {
        Task<bool> RefundPayment(string clientReference, string pspReference, decimal amount);
        Task SearchPayments(string clientReference, int daysAgo);
    }
}