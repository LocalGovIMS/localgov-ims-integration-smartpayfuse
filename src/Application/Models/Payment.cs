using System.Collections.Generic;
using System.Linq;

namespace Application.Models
{
    public class Payment
    {
        public string HppUrl { get; set; }
        public string HmacKey { get; set; }
        public string ShipBeforeDate { get; set; }
        public string SkinCode { get; set; }
        public string MerchantAccount { get; set; }
        public string SessionValidity { get; set; }
        public string CurrencyCode { get; set; }
        public string ShopperLocale { get; set; }
        public string MerchantReference { get; set; }
        public string PaymentAmount { get; set; }
        public string OrderData { get; set; }
        public string CountryCode { get; set; }
        public string ShopperEmail { get; set; }
        public string ShopperReference { get; set; }
        public string AllowedMethods { get; set; }
        public string BlockedMethods { get; set; }
        public string Offset { get; set; }
        public string ResUrl { get; set; }
        public string MerchantSig { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public bool ManuallyStopRecording { get; set; }
        public string PaymentMopCode { get; set; }

        public Dictionary<string, string> SigningParams => new()
        {
            { "currencyCode", CurrencyCode },
            { "merchantAccount", MerchantAccount },
            { "merchantReference", MerchantReference },
            { "paymentAmount", PaymentAmount },
            { "recurringContract", "" },
            { "sessionValidity", SessionValidity },
            { "shipBeforeDate", ShipBeforeDate },
            { "shopperEmail", ShopperEmail },
            { "shopperLocale", ShopperLocale },
            { "shopperReference", ShopperReference },
            { "skinCode", SkinCode },
            { "resURL", ResUrl }
        };

        public void CreateMerchantSignature(
            string cardSelfServiceMopCode,
            string merchantAccount,
            string merchantAccountCNP,
            string hmacKey)
        {
            MerchantAccount = PaymentMopCode == cardSelfServiceMopCode
                ? merchantAccount
                : merchantAccountCNP;

            var signingParams = SigningParams;

            if (BillingAddress != null)
            {
                BillingAddress.CreateMerchantSignature(hmacKey);
                signingParams.Add("billingAddressType", BillingAddress.Type);
                BillingAddress.SigningParams.ToList().ForEach(x => signingParams.Add(x.Key, x.Value));
                signingParams.Add("billingAddressSig", BillingAddress.MerchantSig);
            }

            MerchantSig = HmacUtilities.CalculateHmac(hmacKey, SigningUtilities.BuildSigningString(signingParams));
        }
    }
}
