using Application.Clients.LocalGovImsPaymentApi;
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

namespace Application.Commands
{
    public class PaymentResponseCommand : IRequest<ProcessPaymentResponseModel>
    {
        public Dictionary<string, string> Paramaters { get; set; }
        public PaymentResponse paymentResponse { get; set; }
    }

    public class PaymentResponseCommandHandler : IRequestHandler<PaymentResponseCommand, ProcessPaymentResponseModel>
    {
        private readonly IConfiguration _configuration;
        private readonly ILocalGovImsPaymentApiClient _localGovImsPaymentApiClient;

        private ProcessPaymentModel _processPaymentModel;
        private ProcessPaymentResponseModel _processPaymentResponseModel;

        public PaymentResponseCommandHandler(
            IConfiguration configuration,
            ILocalGovImsPaymentApiClient localGovImsPaymentApiClient)
        {
            _configuration = configuration;
            _localGovImsPaymentApiClient = localGovImsPaymentApiClient;
        }

        public async Task<ProcessPaymentResponseModel> Handle(PaymentResponseCommand request, CancellationToken cancellationToken)
        {
            ValidateRequest(request);

            BuildProcessPaymentModel(request.Paramaters, request.paymentResponse);

            await ProcessPayment();

            return _processPaymentResponseModel;
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

        private void BuildProcessPaymentModel(Dictionary<string, string> paramaters, PaymentResponse paymentResponse)
        {

            switch (paramaters[Keys.AuthorisationResult])
            {
                case AuthorisationResult.Authorised:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = LocalGovIMSResults.Authorised,
                        PspReference = paramaters.GetValueOrDefault(Keys.PspReference),
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference),
                        PaymentMethod = paramaters.GetValueOrDefault(Keys.PaymentMethod)
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

        private async Task ProcessPayment()
        {
            _processPaymentResponseModel = await _localGovImsPaymentApiClient.ProcessPayment(_processPaymentModel.MerchantReference, _processPaymentModel);
        }
    }
}
