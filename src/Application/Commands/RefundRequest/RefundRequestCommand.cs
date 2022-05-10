using Application.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using Application.Clients.LocalGovImsPaymentApi;

namespace Application.Commands
{
    public class RefundRequestCommand : IRequest<RefundResult>
    {
        public Refund Refund { get; set; }
    }

    public class RefundRequestCommandHandler : IRequestHandler<RefundRequestCommand, RefundResult>
    {
        private readonly IConfiguration _configuration;
        private readonly ICybersourceRestApiClient _cybersourceRestApiClient;
        private readonly ILocalGovImsPaymentApiClient _localGovImsPaymentApiClient;

        private decimal _amount;

        public RefundRequestCommandHandler(
            IConfiguration configuration,
            ICybersourceRestApiClient cybersourceRestApiClient,
            ILocalGovImsPaymentApiClient localGovImsPaymentApiClient)
        {
            _configuration = configuration;
            _cybersourceRestApiClient = cybersourceRestApiClient;
            _localGovImsPaymentApiClient = localGovImsPaymentApiClient;
        }

        public async Task<RefundResult> Handle(RefundRequestCommand request, CancellationToken cancellationToken)
        {
            SetAmount(request.Refund);

            var result = await RequestRefund(request.Refund);

            return result 
                ? RefundResult.Successful(request.Refund.Reference, _amount)
                : RefundResult.Failure(string.Empty);
        }

        private void SetAmount(Refund refund)
        {
            _amount = refund.Amount;
        }

        private async Task<bool> RequestRefund(Refund refund)
        {
            var transactions = await _localGovImsPaymentApiClient.GetProcessedTransactions(refund.Reference);
            return await _cybersourceRestApiClient.RefundPayment(refund.Reference, transactions.First().PspReference, refund.Amount);
        }
    }
}
