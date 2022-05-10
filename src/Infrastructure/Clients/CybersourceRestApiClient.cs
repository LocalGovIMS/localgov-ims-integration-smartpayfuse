using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Application.Clients.CybersourceRestApiClient.Interfaces;
using CyberSource.Api;
using CyberSource.Client;
using CyberSource.Model;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Clients
{
    public class CybersourceRestApiClient : ICybersourceRestApiClient
    {
        private readonly string _restApiEndpoint;
        private readonly string _merchantId;
        private readonly string _restSharedSecretId;
        private readonly string _restSharedSecretKey;

        private readonly Dictionary<string, string> _configurationDictionary = new ();

        public CybersourceRestApiClient(IConfiguration configuration)
        {
            _restApiEndpoint = configuration.GetValue<string>("RestApiEndpoint");
            _merchantId = configuration.GetValue<string>("MerchantId");
            _restSharedSecretId = configuration.GetValue<string>("RestSharedSecretId");
            _restSharedSecretKey = configuration.GetValue<string>("RestSharedSecretKey");
            
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
                return result.Status == "PENDING"; // todo: check if extra statuses mean success
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API : " + e.Message);
                return false;
            }
        }

        public async Task SearchPayments(string clientReference, int daysAgo)
        {
            var requestObj = new CreateSearchRequest(
                Save: false,
                Name: "MRN",
                Timezone: "Europe/London",
                Query: BuildSearchQuery(clientReference, daysAgo),
                Offset: 0,
                Limit: 1000,
                Sort: "submitTimeUtc:desc"
            );

            try
            {
                var clientConfig = new Configuration(merchConfigDictObj: _configurationDictionary);

                var apiInstance = new SearchTransactionsApi(clientConfig);
                var result = await apiInstance.CreateSearchAsync(requestObj);
                return; // todo: return correct type
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on calling the API : " + e.Message);
                return;
            }
        }

        private static string BuildSearchQuery(string clientReference, int daysAgo)
        {
            var query = "";
            var submitTimeUtcQuery = "[NOW/DAY-" + daysAgo + "DAY" + ((daysAgo > 1) ? "S" : "") + " TO NOW/DAY+1DAY}";

            if (clientReference != "")
            {
                query = "clientReferenceInformation.code:" + clientReference +
                        " AND submitTimeUtc:" + submitTimeUtcQuery;
            }
            else
            {
                query = "submitTimeUtc:" + submitTimeUtcQuery;
            }

            return query;
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