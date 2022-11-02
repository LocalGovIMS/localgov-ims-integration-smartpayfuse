using Application.Builders;
using LocalGovImsApiClient.Model;
using Application.Cryptography;
using Application.Models;
using Domain.Exceptions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LocalGovImsApiClient.Client;
using Application.Data;
using Application.Entities;
using System;

namespace Application.Commands
{
    public class PaymentRequestCommand : IRequest<SmartPayFusePayment>
    {
        public string Reference { get; set; }

        public string Hash { get; set; }
    }

    public class PaymentRequestCommandHandler : IRequestHandler<PaymentRequestCommand, SmartPayFusePayment>
    {
        private readonly ICryptographyService _cryptographyService;
        private readonly IBuilder<PaymentBuilderArgs, SmartPayFusePayment> _paymentBuilder;
        private readonly LocalGovImsApiClient.Api.IPendingTransactionsApi _pendingTransactionsApi;
        private readonly LocalGovImsApiClient.Api.IProcessedTransactionsApi _processedTransactionsApi;
        private readonly IAsyncRepository<Payment> _paymentRepository;

        private List<PendingTransactionModel> _pendingTransactions;
        private PendingTransactionModel _pendingTransaction;
        private Payment _payment;
        private SmartPayFusePayment _result;

        public PaymentRequestCommandHandler(
            ICryptographyService cryptographyService,
            IBuilder<PaymentBuilderArgs, SmartPayFusePayment> paymentBuilder,
            LocalGovImsApiClient.Api.IPendingTransactionsApi pendingTransactionsApi,
            LocalGovImsApiClient.Api.IProcessedTransactionsApi processedTransactionsApi,
            IAsyncRepository<Payment> paymentRepository)
        {
            _cryptographyService = cryptographyService;
            _paymentBuilder = paymentBuilder;
            _pendingTransactionsApi = pendingTransactionsApi;
            _processedTransactionsApi = processedTransactionsApi;
            _paymentRepository = paymentRepository;
        }

        public async Task<SmartPayFusePayment> Handle(PaymentRequestCommand request, CancellationToken cancellationToken)
        {
            await ValidateRequest(request);

            GetPendingTransaction();

            await CreatePayment(request);

            BuildPayment(request);

            return _result;
        }

        private async Task ValidateRequest(PaymentRequestCommand request)
        {
            ValidateRequestValue(request);
            await CheckThatProcessedTransactionsDoNotExist(request);
            await CheckThatAPendingTransactionExists(request);
        }

        private void ValidateRequestValue(PaymentRequestCommand request)
        {
            if (string.IsNullOrEmpty(request.Reference))
            {
                throw new PaymentException("The reference provided is null or empty");
            }

            if (string.IsNullOrEmpty(request.Hash))
            {
                throw new PaymentException("The hash provided is null or empty");
            }

            if (request.Hash != _cryptographyService.GetHash(request.Reference))
            {
                throw new PaymentException("The hash is invalid");
            }
        }

        private async Task CheckThatProcessedTransactionsDoNotExist(PaymentRequestCommand request)
        {
            try
            {
                var processedTransactions = await _processedTransactionsApi.ProcessedTransactionsSearchAsync(
                    string.Empty,
                    null,
                    string.Empty,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    request.Reference,
                    string.Empty);

                if (processedTransactions != null && processedTransactions.Any())
                {
                    throw new PaymentException("The reference provided is no longer a valid pending payment");
                }
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 404) return; // If no processed transactions are found the API will return a 404 (Not Found) - so that's fine

                throw;
            }
        }

        private async Task CheckThatAPendingTransactionExists(PaymentRequestCommand request)
        {
            try
            {
                var result = await _pendingTransactionsApi.PendingTransactionsGetAsync(request.Reference);

                if (result == null || !result.Any())
                {
                    throw new PaymentException("The reference provided is no longer a valid pending payment");
                }

                _pendingTransactions = result.ToList();
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 404)
                    throw new PaymentException("The reference provided is no longer a valid pending payment");

                throw;
            }
        }

        private void GetPendingTransaction()
        {
            _pendingTransaction = _pendingTransactions.FirstOrDefault();
        }


        private async Task CreatePayment(PaymentRequestCommand request)
        {
            _payment = (await _paymentRepository.Add(new Payment()
            {
                Amount = Convert.ToDecimal(_pendingTransactions.Sum(x => x.Amount)),
                CreatedDate = DateTime.Now,
                Identifier = Guid.NewGuid(),
                Reference = request.Reference,
                FailureUrl = _pendingTransaction.FailUrl
            })).Data;
        }

        private void BuildPayment(PaymentRequestCommand request)
        {
            _result = _paymentBuilder.Build(new PaymentBuilderArgs()
            {
                Reference = request.Reference,
                Amount = _pendingTransactions.Sum(x => x.Amount ?? 0),
                Transaction = _pendingTransaction
            });
        }
    }
}
