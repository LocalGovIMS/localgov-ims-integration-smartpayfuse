using Application.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using SmartPayService;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class RefundRequestCommand : IRequest<RefundResult>
    {
        public Refund Refund { get; set; }
    }

    public class RefundRequestCommandHandler : IRequestHandler<RefundRequestCommand, RefundResult>
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentPortTypeClient _paymentPortTypeClient;

        private Amount _amount;
        private string _merchantAccount;

        public RefundRequestCommandHandler(
            IConfiguration configuration,
            IPaymentPortTypeClient paymentPortTypeClient)
        {
            _paymentPortTypeClient = paymentPortTypeClient;
            _configuration = configuration;
        }

        public async Task<RefundResult> Handle(RefundRequestCommand request, CancellationToken cancellationToken)
        {
            SetSecurityProtocol();
            SetAmount(request.Refund);
            SetMerchantAccount();

            var result = await RequestRefund(request.Refund);

            return result.response == "[refund-received]"
                ? RefundResult.Successful(result.pspReference, _amount.value)
                : RefundResult.Failure(string.Empty);
        }

        private static void SetSecurityProtocol()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private void SetAmount(Refund refund)
        {
            _amount = new Amount()
            {
                currency = "GBP",
                value = Convert.ToInt64(refund.Amount * 100)
            };
        }

        private void SetMerchantAccount()
        {
            _merchantAccount = _configuration.GetValue<string>("SmartPay:MerchantAccount");
        }

        private async Task<ModificationResult> RequestRefund(Refund refund)
        {
            return await _paymentPortTypeClient.refundAsync(new ModificationRequest()
            {
                merchantAccount = _merchantAccount,
                modificationAmount = _amount,
                originalReference = refund.Reference,
            });
        }
    }
}
