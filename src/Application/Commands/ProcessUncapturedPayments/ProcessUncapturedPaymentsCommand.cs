using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using System.Threading;
using Application.Entities;
using Microsoft.Extensions.Logging;
using LocalGovImsApiClient.Client;
using System.Linq;
using Domain.Exceptions;
using System;
using LocalGovImsApiClient.Model;
using Application.Data;
using Application.Extensions;

namespace Application.Commands
{
    public class ProcessUncapturedPaymentsCommand : IRequest<ProcessUncapturedPaymentsResult>
    {
        public int DaysAgo;
        public string ClientReference;

        public ProcessUncapturedPaymentsCommand(int daysAgo, string clientReference = "")
        {
            this.DaysAgo = daysAgo;
            this.ClientReference = clientReference;
        }

    }
    public class ProcessUncapturedPaymentsCommandHander : IRequestHandler<ProcessUncapturedPaymentsCommand, ProcessUncapturedPaymentsResult>
    {
        private readonly IConfiguration _configuration;
        private readonly ICybersourceRestApiClient _cybersourceRestApiClient;
        private readonly ILogger<ProcessUncapturedPaymentsCommandHander> _logger;
        private readonly LocalGovImsApiClient.Api.IPendingTransactionsApi _pendingTransactionsApi;
        private readonly LocalGovImsApiClient.Api.IProcessedTransactionsApi _processedTransactionsApi;
        private readonly IAsyncRepository<Payment> _paymentRepository;

        private List<Payment> _uncapturedPayments = new();
        private Payment _uncapturedPayment;
        private ProcessUncapturedPaymentsResult _processUncaturedPaymentResult;
        private int _numberOfErrors = 0;
        private List<PendingTransactionModel> _pendingTransactions;
        private ProcessPaymentResponse _processPaymentResponse;
        private ProcessPaymentModel _processPaymentModel;
        private Payment _payment;


        public ProcessUncapturedPaymentsCommandHander(
            IConfiguration configuration,
            ICybersourceRestApiClient cybersourceRestApiClient,
            ILogger<ProcessUncapturedPaymentsCommandHander> logger,
            LocalGovImsApiClient.Api.IPendingTransactionsApi pendingTransactionsApi,
            IAsyncRepository<Payment> paymentRepository,
            LocalGovImsApiClient.Api.IProcessedTransactionsApi processedTransactionsApi
)
        {
            _configuration = configuration;
            _cybersourceRestApiClient = cybersourceRestApiClient;
            _logger = logger;
            _pendingTransactionsApi = pendingTransactionsApi;
            _paymentRepository = paymentRepository;
            _processedTransactionsApi = processedTransactionsApi;
        }

        public async Task<ProcessUncapturedPaymentsResult> Handle(ProcessUncapturedPaymentsCommand request, CancellationToken cancellationToken)
        {
            await SearchForUncapturedPayments(request.ClientReference, request.DaysAgo);

            await ProcessUncapturedPayments();

            CreateResult();

            return _processUncaturedPaymentResult;
        }

        private async Task SearchForUncapturedPayments(string clientReference, int daysAgo)
        {
            //   var transactions = await _localGovImsPaymentApiClient.GetProcessedTransactions(refund.Reference);
            _uncapturedPayments = await _cybersourceRestApiClient.SearchPayments(clientReference, daysAgo);
        }

        private async Task ProcessUncapturedPayments()
        {
            foreach (var uncapturedPayment in _uncapturedPayments)
            {
                _uncapturedPayment = uncapturedPayment;

                await ProcessUncapturedPayment();
            }

            _logger.LogInformation(_uncapturedPayments.Count + " rows processed");
            _logger.LogInformation(_numberOfErrors + " failures. See logs for more details");
        }

        private async Task ProcessUncapturedPayment()
        {
            try
            {
                await GetIntegrationPayment();

                 if (_payment.CardDetailsNeedUpdating())
                {
                    await SetUpCardDetailsAsync();
                    return;
                }

                await GetPendingTransactions();

                BuildProcessPaymentModel();

                await ProcessPayment();

                await SetIntegrationPaymentAsFinished();
            }
            catch (Exception ex)
            {
                _numberOfErrors++;

                _logger.LogError(ex, "Unable to process uncaptured payment record: " + _uncapturedPayment.Id);
            }
        }
        private async Task GetIntegrationPayment( )
        {

            _payment = (await _paymentRepository.Get(x => x.Reference == _uncapturedPayment.PaymentId && x.Finished == false)).Data;
            if (_payment == null )
            {
                throw new PaymentException("The reference provided does not exist");
            }
        }

  
    private async Task SetUpCardDetailsAsync()
        {
            var cardDetails = new UpdateCardDetailsModel
            { 
                CardPrefix = _uncapturedPayment.CardPrefix,
                CardSuffix = _uncapturedPayment.CardSuffix,
                MerchantReference = _uncapturedPayment.PaymentId
            };
            await UpdateCardDetails("", cardDetails);
        }

        private async Task UpdateCardDetails(string reference, UpdateCardDetailsModel cardDetails)
        {
            await _processedTransactionsApi.ProcessedTransactionsUpdateCardDetailsAsync(_payment.Reference, cardDetails);
        }

        private async Task GetPendingTransactions()
        {
            try
            {
                _pendingTransactions = (await _pendingTransactionsApi.PendingTransactionsGetAsync(_uncapturedPayment.PaymentId)).ToList();

                if (_pendingTransactions == null || !_pendingTransactions.Any())
                {
                    throw new PaymentException("The reference provided is no longer a valid pending payment");
                }
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 404)
                    throw new PaymentException("The reference provided is no longer a valid pending payment");

                throw;
            }
        }
        private void BuildProcessPaymentModel()
        {
            _processPaymentModel = new ProcessPaymentModel()
            {
                AuthResult = LocalGovIMSResults.Pending,
                PspReference = _uncapturedPayment.Reference,
                MerchantReference = _uncapturedPayment.PaymentId,
                Fee = 0,
                CardPrefix = _uncapturedPayment.CardPrefix,
                CardSuffix = _uncapturedPayment.CardSuffix
            };
        }

        private async Task ProcessPayment()
        {
            _processPaymentResponse = await _pendingTransactionsApi.PendingTransactionsProcessPaymentAsync(_uncapturedPayment.PaymentId, _processPaymentModel);
        }

        private void CreateResult()
        {
            _processUncaturedPaymentResult = new ProcessUncapturedPaymentsResult()
            {
                TotalIdentified = _uncapturedPayments.Count,
                TotalMarkedAsCaptured = _uncapturedPayments.Count - _numberOfErrors,
                TotalErrors = _numberOfErrors
            };
        }

        private async Task SetIntegrationPaymentAsFinished()
        {
            _payment.Finished = true;
            _payment = (await _paymentRepository.Update(_payment)).Data;
        }
    }
}
