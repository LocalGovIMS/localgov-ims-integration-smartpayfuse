using Application.Clients.LocalGovImsPaymentApi;
using Application.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Application.Builders
{
    public class PaymentBuilder : IBuilder<PaymentBuilderArgs, Payment>
    {
        private readonly IConfiguration _configuration;

        private Payment _payment;

        public PaymentBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Payment Build(PaymentBuilderArgs args)
        {
            CreatePayment(args);

            SetMopCode(args);

            SetSessionValidity(args);

            CreateMerchantSignature(args);

            return _payment;
        }

        private void CreatePayment(PaymentBuilderArgs args)
        {
            _payment = new Payment
            {
                PaymentAmount = ((int)(args.Amount * 100)).ToString(),
                MerchantReference = args.Reference,
                HppUrl = _configuration.GetValue<string>("SmartPay:Url"),
                HmacKey = _configuration.GetValue<string>("SmartPay:HmacKey"),
                ShipBeforeDate = DateTime.Today.ToString("yyyy-MM-dd"),
                SkinCode = _configuration.GetValue<string>("SmartPay:SkinCode"),
                SessionValidity = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:ssK"),
                ResUrl = _configuration.GetValue<string>("PaymentPortalUrl") + "/Payment/PaymentResponse",
                CurrencyCode = "GBP",
                ShopperLocale = "en_GB",
                BillingAddress = new BillingAddress()
                {
                    HouseNumberOrName = args.Transaction.PayeePremiseNumber,
                    Street = args.Transaction.PayeeStreet,
                    City = args.Transaction.PayeeTown,
                    PostCode = args.Transaction.PayeePostCode,
                    County = args.Transaction.PayeeCounty
                }
            };
        }

        private void SetMopCode(PaymentBuilderArgs args)
        {
            if (args.Transaction?.MopCode != null)
            {
                _payment.PaymentMopCode = args.Transaction.MopCode;
            }
        }

        private void SetSessionValidity(PaymentBuilderArgs args)
        {
            if (args.Transaction?.ExpiryDate != null)
            {
                _payment.SessionValidity = ((DateTime)args.Transaction.ExpiryDate).ToString("yyyy-MM-ddTHH:mm:ssK");
            }
        }

        private void CreateMerchantSignature(PaymentBuilderArgs args)
        {
            _payment.CreateMerchantSignature(
                args.CardSelfServiceMopCode,
                _configuration.GetValue<string>("SmartPay:MerchantAccount"),
                _configuration.GetValue<string>("SmartPay:MerchantAccountCNP"),
                _configuration.GetValue<string>("SmartPay:HmacKey"));
        }
    }

    public class PaymentBuilderArgs
    {
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string CardSelfServiceMopCode { get; set; }
        public PendingTransactionModel Transaction { get; set; }
    }
}
