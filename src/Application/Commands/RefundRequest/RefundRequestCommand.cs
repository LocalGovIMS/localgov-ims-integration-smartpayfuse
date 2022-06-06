using Application.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using Application.Data;
using Application.Entities;
using System;

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
        private readonly IAsyncRepository<Payment> _paymentRepository;

        private decimal _amount;
        private Payment _payment;

        public RefundRequestCommandHandler(
            IConfiguration configuration,
            ICybersourceRestApiClient cybersourceRestApiClient,
            IAsyncRepository<Payment> paymentRepository)
        {
            _configuration = configuration;
            _cybersourceRestApiClient = cybersourceRestApiClient;
            _paymentRepository = paymentRepository;
        }

        public async Task<RefundResult> Handle(RefundRequestCommand request, CancellationToken cancellationToken)
        {
            await CreateIntegrationTransactionAsync(request);

            SetAmount(request.Refund);

            var result = await RequestRefund(request.Refund);

            return result 
                ? RefundResult.Successful(request.Refund.Reference, _amount)
                : RefundResult.Failure(string.Empty);
        }

        private async Task CreateIntegrationTransactionAsync(RefundRequestCommand request)
        {
            _payment = (await _paymentRepository.Add(new Payment()
            {
                Amount = request.Refund.Amount,
                CreatedDate = DateTime.Now,
                Identifier = Guid.NewGuid(),
                Reference = request.Refund.Reference,
                PaymentId = "Refund"
            })).Data;
        }

        private void SetAmount(Refund refund)
        {
            _amount = refund.Amount;
        }

        private async Task<bool> RequestRefund(Refund refund)
        {
            return await _cybersourceRestApiClient.RefundPayment(refund.Reference, refund.Reference, refund.Amount);
        }
    }
}
