using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using Application.Commands;
using CyberSource.Api;
using CyberSource.Client;
using CyberSource.Model;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Application.Entities;
using Infrastructure.Extensions;

namespace Infrastructure.Clients
{
    public class CybersourceRestApiClient : ICybersourceRestApiClient
    {
        private readonly string _restApiEndpoint;
        private readonly string _merchantId;
        private readonly string _restSharedSecretId;
        private readonly string _restSharedSecretKey;
        private List<Payment> _uncapturedPayments = new();

        private readonly Dictionary<string, string> _configurationDictionary = new ();

        public CybersourceRestApiClient(IConfiguration configuration)
        {
            _restApiEndpoint = configuration.GetValue<string>("SmartPayFuse:RestApiEndpoint");
            _merchantId = configuration.GetValue<string>("SmartPayFuse:MerchantId");
            _restSharedSecretId = configuration.GetValue<string>("SmartPayFuse:RestSharedSecretId");
            _restSharedSecretKey = configuration.GetValue<string>("SmartPayFuse:RestSharedSecretKey");
            
            SetupConfigDictionary();
        }

        public async Task<bool> RefundPayment(string clientReference, string pspReference, decimal amount)
        {
            try
            {
                var clientConfig = new Configuration(merchConfigDictObj: _configurationDictionary);
                
                var clientReferenceInformation = new Ptsv2paymentsClientReferenceInformation(
                    Code: clientReference
                );

                var orderInformationAmountDetails = new Ptsv2paymentsidcapturesOrderInformationAmountDetails(
                    TotalAmount: amount.ToString(CultureInfo.InvariantCulture),
                    Currency: "GBP"
                );

                var orderInformation = new Ptsv2paymentsidrefundsOrderInformation(
                    AmountDetails: orderInformationAmountDetails
                );

                var requestObj = new RefundPaymentRequest(
                    ClientReferenceInformation: clientReferenceInformation,
                    OrderInformation: orderInformation
                );

                var apiInstance = new RefundApi(clientConfig);
                var result = await apiInstance.RefundPaymentAsync(requestObj, pspReference);
                return result.Status == LocalGovIMSResults.Pending; 
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API : " + e.Message);
                return false;
            }
        }

        public async Task<List<Payment>> SearchPayments(string clientReference, int daysAgo)
        {
            List<Payment> _uncapturedPayments = new();

            var requestObj = CybersourceExtensions.CreateNewSearchRequest(clientReference, daysAgo);

            try
            {
                var clientConfig = new Configuration(merchConfigDictObj: _configurationDictionary);

                var apiInstance = new SearchTransactionsApi(clientConfig);
                var searchResult = await apiInstance.CreateSearchAsync(requestObj);

                if (searchResult == null || searchResult.Count == 0)
                    return _uncapturedPayments;

                if (searchResult.Embedded.TransactionSummaries.All(x
                        => string.IsNullOrWhiteSpace(x.ProcessorInformation.ApprovalCode)))
                    return _uncapturedPayments;

                var activeResults = searchResult.Embedded.TransactionSummaries.Where(x =>
                    !string.IsNullOrWhiteSpace(x.ProcessorInformation.ApprovalCode));

                foreach (var matchingResult in activeResults)
                {
                    _uncapturedPayments.Add(CybersourceExtensions.CreatePaymentRecord(matchingResult));
                }
                return _uncapturedPayments; 
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API : " + e.Message);
                return _uncapturedPayments; // TODO: fix
            }
        }

        public async Task<List<Payment>> SearchRefunds(string clientReference, int daysAgo)
        {
            List<Payment> _uncapturedPayments = new();

            var requestObj = CybersourceExtensions.CreateNewSearchRequest(clientReference, daysAgo);

            try
            {
                var clientConfig = new Configuration(merchConfigDictObj: _configurationDictionary);

                var apiInstance = new SearchTransactionsApi(clientConfig);
                var searchResult = await apiInstance.CreateSearchAsync(requestObj);

                if (searchResult == null || searchResult.Count != 1)
                    return _uncapturedPayments;

                if (searchResult.Embedded.TransactionSummaries.All(x => x.ApplicationInformation.RFlag != "SOK"))
                    return _uncapturedPayments;

                var activeResults = searchResult.Embedded.TransactionSummaries;

                foreach (var matchingResult in activeResults)
                {
                    _uncapturedPayments.Add(CybersourceExtensions.CreatePaymentRecord(matchingResult));
                }
                return _uncapturedPayments;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API : " + e.Message);
                return _uncapturedPayments; // TODO: fix
            }
        }

        private void SetupConfigDictionary()
        {
            // General configuration
            _configurationDictionary.Add("authenticationType", "HTTP_SIGNATURE");
            _configurationDictionary.Add("merchantID", _merchantId);
            _configurationDictionary.Add("merchantKeyId", _restSharedSecretId);
            _configurationDictionary.Add("merchantsecretKey", _restSharedSecretKey);
            _configurationDictionary.Add("runEnvironment", _restApiEndpoint);
            _configurationDictionary.Add("timeout", "300000");

            // Configs related to meta key
            _configurationDictionary.Add("portfolioID", string.Empty);
            _configurationDictionary.Add("useMetaKey", "false");

            // Configs related to OAuth
            _configurationDictionary.Add("enableClientCert", "false");
        }
    }
}