using Application.Builders;
using Application.Clients.LocalGovImsPaymentApi;
using Application.Cryptography;
using Application.Models;
using Domain.Exceptions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class PaymentRequestCommand : IRequest<Payment>
    {
        public string Reference { get; set; }

        public string Hash { get; set; }
    }

    public class PaymentRequestCommandHandler : IRequestHandler<PaymentRequestCommand, Payment>
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly ILocalGovImsPaymentApiClient _localGovImsPaymentApiClient;
        private readonly IBuilder<PaymentBuilderArgs, Payment> _paymentBuilder;

        private List<PendingTransactionModel> _pendingTransactions;
        private PendingTransactionModel _pendingTransaction;
        private Payment _payment;

        public PaymentRequestCommandHandler(
            ICryptographyService cryptographyService,
            ILocalGovImsPaymentApiClient localGovImsPaymentApiClient,
            IBuilder<PaymentBuilderArgs, Payment> paymentBuilder)
        {
            _cryptographyService = cryptographyService;
            _localGovImsPaymentApiClient = localGovImsPaymentApiClient;
            _paymentBuilder = paymentBuilder;
        }

        public async Task<Payment> Handle(PaymentRequestCommand request, CancellationToken cancellationToken)
        {
            await ValidateRequest(request);

            GetPendingTransaction();

            await BuildPayment(request);

            return _payment;
        }

        private async Task ValidateRequest(PaymentRequestCommand request)
        {
            if (request.Reference == null || request.Hash == null || request.Hash != _cryptographyService.GetHash(request.Reference))
            {
                throw new PaymentException("The reference provided is not valid");
            }

            var processedTransactions = await _localGovImsPaymentApiClient.GetProcessedTransactions(request.Reference);
            if (processedTransactions != null && processedTransactions.Any())
            {
                throw new PaymentException("The reference provided is no longer a valid pending payment");
            }

            _pendingTransactions = await _localGovImsPaymentApiClient.GetPendingTransactions(request.Reference);
            if (_pendingTransactions == null || !_pendingTransactions.Any())
            {
                throw new PaymentException("The reference provided is no longer a valid pending payment");
            }
        }

        private void GetPendingTransaction()
        {
            _pendingTransaction = _pendingTransactions.FirstOrDefault();
        }

        private async Task BuildPayment(PaymentRequestCommand request)
        {
            _payment = _paymentBuilder.Build(new PaymentBuilderArgs()
            {
                Reference = request.Reference,
                Amount = _pendingTransactions.Sum(x => x.Amount ?? 0),
                CardSelfServiceMopCode = (await _localGovImsPaymentApiClient.GetCardSelfServiceMopCode()).Code,
                Transaction = _pendingTransaction
            });
        }
    }
}
