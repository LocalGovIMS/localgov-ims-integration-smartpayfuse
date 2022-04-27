using Application.Clients.LocalGovImsPaymentApi;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Keys = Application.Commands.PaymentResponseParameterKeys;

namespace Application.Commands
{
    public class PaymentResponseCommand : IRequest<ProcessPaymentResponseModel>
    {
        public Dictionary<string, string> Paramaters { get; set; }
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

            BuildProcessPaymentModel(request.Paramaters);

            await ProcessPayment();

            return _processPaymentResponseModel;
        }

        private void ValidateRequest(PaymentResponseCommand request)
        {
            var originalMerchantSignature = ExtractMerchantSignature(request.Paramaters);
            var calculatedMerchantSignature = CalculateMerchantSignature(request.Paramaters);

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

        private string CalculateMerchantSignature(Dictionary<string, string> paramaters)
        {
            string signingString = SigningUtilities.BuildSigningString(paramaters);
            string calculatedMerchantSignature = HmacUtilities.CalculateHmac(_configuration.GetValue<string>("SmartPay:HmacKey"), signingString);

            return calculatedMerchantSignature;
        }

        private void BuildProcessPaymentModel(Dictionary<string, string> paramaters)
        {
            switch (paramaters[Keys.AuthorisationResult])
            {
                case AuthorisationResult.Authorised:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = paramaters.GetValueOrDefault(Keys.AuthorisationResult),
                        PspReference = paramaters.GetValueOrDefault(Keys.PspReference),
                        MerchantReference = paramaters.GetValueOrDefault(Keys.MerchantReference),
                        PaymentMethod = paramaters.GetValueOrDefault(Keys.PaymentMethod)
                    };
                    break;

                default:
                    _processPaymentModel = new ProcessPaymentModel()
                    {
                        AuthResult = paramaters.GetValueOrDefault(Keys.AuthorisationResult),
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
