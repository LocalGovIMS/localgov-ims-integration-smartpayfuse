using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
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

namespace Application.Commands
{
    public class ProcessUncapturedRefundsCommand : IRequest<ProcessUncapturedRefundsCommandResult>
    {
        public int DaysAgo;
        public string ClientReference;

        public ProcessUncapturedRefundsCommand(int daysAgo, string clientReference = "")
        {
            this.DaysAgo = daysAgo;
            this.ClientReference = clientReference;
        }

    }
    public class ProcessUncapturedRefundsCommandHander : IRequestHandler<ProcessUncapturedRefundsCommand, ProcessUncapturedRefundsCommandResult>
    {
        private readonly ICybersourceRestApiClient _cybersourceRestApiClient;
        private readonly ILogger<ProcessUncapturedRefundsCommandHander> _logger;
        private readonly LocalGovImsApiClient.Api.IPendingTransactionsApi _pendingTransactionsApi;
        private readonly IAsyncRepository<Payment> _paymentRepository;

        private List<Payment> _uncapturedRefundsPayments = new();
        private List<Payment> _uncapturedCyberSourceRefunds = new();
        private Payment _uncapturedRefund;
        private ProcessUncapturedRefundsCommandResult _processUncaturedRefundCommandResult;
        private int _numberOfErrors = 0;
        private List<PendingTransactionModel> _pendingTransactions;
        private ProcessPaymentResponse _processPaymentResponse;
        private ProcessPaymentModel _processPaymentModel;


        public ProcessUncapturedRefundsCommandHander(
            ICybersourceRestApiClient cybersourceRestApiClient,
            ILogger<ProcessUncapturedRefundsCommandHander> logger,
            LocalGovImsApiClient.Api.IPendingTransactionsApi pendingTransactionsApi,
            IAsyncRepository<Payment> paymentRepository
)
        {
            _cybersourceRestApiClient = cybersourceRestApiClient;
            _logger = logger;
            _pendingTransactionsApi = pendingTransactionsApi;
            _paymentRepository = paymentRepository;
        }

        public async Task<ProcessUncapturedRefundsCommandResult> Handle(ProcessUncapturedRefundsCommand request, CancellationToken cancellationToken)
        {

            await GetRefundTransactions();

            await ProcessUncapturedRefunds();

            CreateResult();

            return _processUncaturedRefundCommandResult;
        }

        private async Task GetRefundTransactions()
        {
           _uncapturedRefundsPayments = (await _paymentRepository.List(x => x.RefundReference != null && x.Finished != true)).Data;
        }

  
        private async Task ProcessUncapturedRefunds()
        {
            foreach (var uncapturedRefund in _uncapturedRefundsPayments)
            {
                _uncapturedRefund = uncapturedRefund;
                _uncapturedCyberSourceRefunds = await _cybersourceRestApiClient.SearchRefunds(uncapturedRefund.Reference, 1);

                if (_uncapturedCyberSourceRefunds != null && _uncapturedCyberSourceRefunds.Any())
                {
                    _uncapturedRefund.CardPrefix = _uncapturedCyberSourceRefunds.FirstOrDefault().CardPrefix;
                    _uncapturedRefund.CardSuffix = _uncapturedCyberSourceRefunds.FirstOrDefault().CardSuffix;
                    _uncapturedRefund.PaymentId = _uncapturedCyberSourceRefunds.FirstOrDefault().PaymentId;
                    await ProcessUncapturedRefund();
                }
            }

            _logger.LogInformation(_uncapturedRefundsPayments.Count + " rows processed");
            _logger.LogInformation(_numberOfErrors + " failures. See logs for more details");
        }

        private async Task ProcessUncapturedRefund()
        {
            try
            {
                await GetPendingTransactions();

                BuildProcessPaymentModel();

                await ProcessPayment();

                await UpdateIntegrationStatus();
            }
            catch (Exception ex)
            {
                _numberOfErrors++;

                _logger.LogError(ex, "Unable to process uncaptured payment record: " + _uncapturedRefund.Id);
            }
        }

        private async Task GetPendingTransactions()
        {
            try
            {
                _pendingTransactions = (await _pendingTransactionsApi.PendingTransactionsGetAsync(_uncapturedRefund.Reference)).ToList();

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
                AuthResult = LocalGovIMSResults.Authorised,
                PspReference = _uncapturedRefund.PaymentId,
                MerchantReference = _uncapturedRefund.Reference,
                Fee = 0,
                CardPrefix = _uncapturedRefund.CardPrefix,
                CardSuffix = _uncapturedRefund.CardSuffix,
                AmountPaid = _uncapturedCyberSourceRefunds.FirstOrDefault().Amount
            };
        }

        private async Task ProcessPayment()
        {
            _processPaymentResponse = await _pendingTransactionsApi.PendingTransactionsProcessPaymentAsync(_uncapturedRefund.Reference, _processPaymentModel);
        }

        private async Task UpdateIntegrationStatus()
        {
            _uncapturedRefund.Status = _processPaymentModel.AuthResult;
            _uncapturedRefund.CapturedDate = DateTime.Now;
            _uncapturedRefund.Finished = true;
            _uncapturedRefund = (await _paymentRepository.Update(_uncapturedRefund)).Data;
        }

        private void CreateResult()
        {
            _processUncaturedRefundCommandResult = new ProcessUncapturedRefundsCommandResult()
            {
                TotalIdentified = _uncapturedRefundsPayments.Count,
                TotalMarkedAsCaptured = _uncapturedRefundsPayments.Count - _numberOfErrors,
                TotalErrors = _numberOfErrors
            };
        }
    }
}
