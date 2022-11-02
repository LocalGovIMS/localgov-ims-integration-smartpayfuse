using System;
using LocalGovImsApiClient.Model;

namespace Application.UnitTests
{
    public partial class TestData
    {
        public static PendingTransactionModel GetPendingTransactionModel()
        {
            return new PendingTransactionModel()
            {
                AccountReference = "AccountReference",
                Amount = 1.00M,
                CancelUrl = "CancelUrl",
                CreatedDate = DateTime.Now,
                ExpiryDate = null,
                ExternalReference = "ExternalReference",
                FailUrl = "FailUrl",
                FundCode = "FF",
                Id = 1,
                InternalReference = "InternalReference",
                MopCode = "MM",
                Narrative = "Narrative",
                OfficeCode = "OO",
                PayeeName = "PayeeName",
                PayeePostCode = "PayeePostCode",
                PayeeAddressLine1 = "PayeeAddressLine1",
                PayeeAddressLine2 = "PayeeAddressLine2",
                PayeeAddressLine3 = "PayeeAddressLine3",
                PayeeAddressLine4 = "PayeeAddressLine4",
                Reference = "Reference",
                StatusId = 1,
                SuccessUrl = "SuccessUrl",
                TransactionDate = DateTime.Now,
                UserCode = 1,
                VatAmount = 0.20M,
                VatCode = "VV",
                VatRate = 20F
            };
        }
    }
}
