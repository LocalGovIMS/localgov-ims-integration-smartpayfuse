using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Keys = Application.Commands.PaymentResponseParameterKeys;
using System;
using System.Text;
using System.Security.Cryptography;
using Application.Models;
using LocalGovImsApiClient.Model;
using Application.Data;
using Application.Entities;
using Application.Clients.CybersourceRestApiClient.Interfaces;

namespace Application.Commands
{
    public class PaymentResponseCommand : IRequest<PaymentResponseCommandResult>
    {
        public Dictionary<string, string> Paramaters { get; set; }
        public PaymentResponse paymentResponse { get; set; }
    }

    public class PaymentResponseCommandHandler : IRequestHandler<PaymentResponseCommand, PaymentResponseCommandResult>
    {
        private readonly IConfiguration _configuration;
        private readonly LocalGovImsApiClient.Api.IPendingTransactionsApi _pendingTransactionsApi;
        private readonly IAsyncRepository<Payment> _paymentRepository;
        private readonly ICybersourceRestApiClient _cybersourceRestApiClient;

        private ProcessPaymentModel _processPaymentModel;
        private ProcessPaymentResponse _processPaymentResponse;
        private PaymentResponseCommandResult _result;
        private Payment _payment;
        private List<Payment> _uncapturedPayments = new();

        public PaymentResponseCommandHandler(
            IConfiguration configuration,
            LocalGovImsApiClient.Api.IPendingTransactionsApi pendingTransactionsApi,
            IAsyncRepository<Payment> paymentRepository,
            ICybersourceRestApiClient cybersourceRestApiClient )
        {
            _configuration = configuration;
            _pendingTransactionsApi = pendingTransactionsApi;
            _paymentRepository = paymentRepository;
            _cybersourceRestApiClient = cybersourceRestApiClient;
        }

        public async Task<PaymentResponseCommandResult> Handle(PaymentResponseCommand request, CancellationToken cancellationToken)
        {
            ValidateRequest(request);

            await BuildProcessPaymentModelAsync(request.Paramaters);

            await GetIntegrationPayment(_processPaymentModel);

            await ProcessPayment();

            await UpdateIntegrationPaymentStatus();

            BuildResult();

            return _result;
        }

        private void ValidateRequest(PaymentResponseCommand request)
        {
            var originalMerchantSignature = ExtractMerchantSignature(request.Paramaters);
            var calculatedMerchantSignature = CalculateMerchantSignature(request.Paramaters, request.paymentResponse);

            if (!calculatedMerchantSignature.Equals(originalMerchantSignature))
            {
                throw new PaymentException("Unable to process the payment");
            }
        }

        private static string ExtractMerchantSignature(Dictionary<string, string> paramaters)
        {
            string originalMerchantSignature = paramaters[Keys.MerchantSignature];

            paramaters.Remove(Keys.MerchantSignature);

            return originalMerchantSignature;
        }

        private string CalculateMerchantSignature(Dictionary<string, string> paramaters, PaymentResponse paymentResponse)
        {
            var signingString = string.Join(",", paymentResponse.Signed_Field_Names.Split(',')
                .Select(signingField => signingField + "=" + paramaters[signingField]).ToList());
                 var encoding = new UTF8Encoding();
                 var keyByte = encoding.GetBytes(_configuration.GetValue<string>("SmartPayFuse:SecretKey"));

                  var hmacsha256 = new HMACSHA256(keyByte);
                  var messageBytes = encoding.GetBytes(signingString);
                  var calculatedMerchantSignature = Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
            return calculatedMerchantSignature;

        }


        private async Task BuildProcessPaymentModelAsync(Dictionary<string, string> paramaters)
        {

            switch (paramaters[Keys.AuthorisationResult])
            {
                case AuthorisationResult.Authorised:
                    var paymentCardDetails = await _cybersourceRestApiClient.SearchPayments(paramaters.GetValueOrDefault(Keys.MerchantReference), 1);
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = LocalGovIMSResults.Authorised,
                        PspReference = paramaters.GetValueOrDefault(Keys.PspReference),
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference),
                        PaymentMethod = paramaters.GetValueOrDefault(Keys.PaymentMethod),
                        CardPrefix = paymentCardDetails.Count == 0 ? null : paymentCardDetails.FirstOrDefault().CardPrefix,
                        CardSuffix = paymentCardDetails.Count == 0 ? null : paymentCardDetails.FirstOrDefault().CardSuffix,
                        AmountPaid = paymentCardDetails.Count == 0 ? 0 : paymentCardDetails.FirstOrDefault().Amount
                    };
                    break;
                case AuthorisationResult.Declined:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = LocalGovIMSResults.Refused,
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference)
                    };
                    break;
                case AuthorisationResult.Cancelled:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = LocalGovIMSResults.Cancelled,
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference)
                    };
                    break;
                default:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = LocalGovIMSResults.Error,
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference)
                    };
                    break;
            }
        }

        private async Task GetIntegrationPayment(ProcessPaymentModel _processPaymentModel)
        {
            _payment = (await _paymentRepository.Get(x => x.Reference == _processPaymentModel.MerchantReference)).Data;
        }

        private async Task UpdateIntegrationPaymentStatus()
        {
            _payment.Status = _processPaymentModel.AuthResult;
            _payment.PaymentId = _processPaymentModel.PspReference;
            _payment.CardPrefix = _processPaymentModel.CardPrefix;
            _payment.CardSuffix = _processPaymentModel.CardSuffix;
            _payment.CapturedDate = DateTime.Now;
            _payment.Finished = true;
            _payment = (await _paymentRepository.Update(_payment)).Data;
        }

        private async Task ProcessPayment()
        {
            _processPaymentResponse = await _pendingTransactionsApi.PendingTransactionsProcessPaymentAsync(_processPaymentModel.MerchantReference, _processPaymentModel);
        }
        private void BuildResult()
        {
            _result = new PaymentResponseCommandResult()
            {
                NextUrl = _processPaymentResponse.RedirectUrl,
                Success = _processPaymentResponse.Success
            };
        }
    }
}
