using LocalGovImsApiClient.Model;
using Application.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using Application.Entities;

namespace Application.Builders
{
    public class PaymentBuilder : IBuilder<PaymentBuilderArgs, SmartPayFusePayment>
    {
        private readonly IConfiguration _configuration;

        private SmartPayFusePayment _payment;

        public PaymentBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SmartPayFusePayment Build(PaymentBuilderArgs args)
        {
            CreatePayment(args);

            CreateMerchantSignature();

            return _payment;
        }

        private void CreatePayment(PaymentBuilderArgs args)
        {
            _payment = new SmartPayFusePayment();
            _payment.AccessKey = _configuration.GetValue<string>("SmartPayFuse:AccessKey");
            _payment.ProfileId = _configuration.GetValue<string>("SmartPayFuse:ProfileId");
            _payment.Amount = args.Amount;
            _payment.ReferenceNumber = args.Reference;
            _payment.SmartPayFusePaymentEndpoint = _configuration.GetValue<string>("SmartPayFuse:HostedCheckoutEndpoint");
            _payment.SecretKey = _configuration.GetValue<string>("SmartPayFuse:SecretKey");
            _payment.OverrideBackofficePostUrl = _configuration.GetValue<string>("PaymentPortalUrl") + "/Payment/PaymentResponse";
            _payment.OverrideCustomCancelPage = _configuration.GetValue<string>("PaymentPortalUrl") + "/Payment/PaymentResponse";
            _payment.OverrideCustomReceiptPage = _configuration.GetValue<string>("PaymentPortalUrl") + "/Payment/PaymentResponse";
            _payment.Currency = "GBP";
            _payment.Locale = "en";
            _payment.BillToAddressLine1 = args.Transaction.PayeePremiseNumber + " " + args.Transaction.PayeeStreet;
            _payment.BillToAddressCity = args.Transaction.PayeeTown;
            _payment.BillToAddressPostalCode = args.Transaction.PayeePostCode;
            _payment.BillToAddressCountry = "GB";

        }

        private void CreateMerchantSignature()
        {
            var signingString = "";

            _payment.SignedDateTime = DateTime.UtcNow;
            signingString += "access_key=" + _payment.AccessKey + ",";
            signingString += "profile_id=" + _payment.ProfileId + ",";
            signingString += "transaction_uuid=" + _payment.TransactionUuid + ",";
            signingString += "signed_field_names=" +
                             "access_key,profile_id,transaction_uuid,signed_field_names,unsigned_field_names,signed_date_time,locale,transaction_type,reference_number,amount,currency,override_backoffice_post_url,override_custom_cancel_page,override_custom_receipt_page" +
                             ",";
            signingString += "unsigned_field_names=" +
                             "bill_to_address_line1,bill_to_address_city,bill_to_address_state,bill_to_address_postal_code,bill_to_address_country,bill_to_email" +
                             ",";
            signingString += "signed_date_time=" + _payment.SignedDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + ",";
            signingString += "locale=" + _payment.Locale + ",";
            signingString += "transaction_type=" + _payment.TransactionType + ",";
            signingString += "reference_number=" + _payment.ReferenceNumber + ",";
            signingString += "amount=" + _payment.Amount + ",";
            signingString += "currency=" + _payment.Currency + ",";
            signingString += "override_backoffice_post_url=" + _payment.OverrideBackofficePostUrl + ",";
            signingString += "override_custom_cancel_page=" + _payment.OverrideCustomCancelPage + ",";
            signingString += "override_custom_receipt_page=" + _payment.OverrideCustomReceiptPage + "";
            var encoding = new UTF8Encoding();
            var keyByte = encoding.GetBytes(_payment.SecretKey);

            var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(signingString);
            var signature = Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));

            _payment.Signature = signature;
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
