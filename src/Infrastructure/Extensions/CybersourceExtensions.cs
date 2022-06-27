using Application.Entities;
using System;
using CyberSource.Model;

namespace Infrastructure.Extensions
{
    public static class CybersourceExtensions
    {

        public static CreateSearchRequest CreateNewSearchRequest(string clientReference, int daysAgo)
        {
            return new CreateSearchRequest(
                Save: false,
                Name: "MRN",
                Timezone: "Europe/London",
                Query: BuildSearchQuery(clientReference, daysAgo),
                Offset: 0,
                Limit: 1000,
                Sort: "submitTimeUtc:desc");
        }

        public static Payment CreatePaymentRecord(TssV2TransactionsPost201ResponseEmbeddedTransactionSummaries result)
        {
            return new Payment
            {
                CreatedDate = DateTime.Now,
                Identifier = Guid.NewGuid(),
                Reference = result.ClientReferenceInformation.Code,
                Amount = decimal.Parse(result.OrderInformation.AmountDetails.TotalAmount),
                PaymentId = result.Id,
                CapturedDate = Convert.ToDateTime(result.SubmitTimeUtc),
                CardPrefix = result.PaymentInformation.Card.Prefix,
                CardSuffix = result.PaymentInformation.Card.Suffix
            };
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
    }
}
