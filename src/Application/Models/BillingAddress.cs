using System;
using System.Collections.Generic;

namespace Application.Models
{
    [Serializable]
    public class BillingAddress
    {
        public string Street { get; set; }
        public string HouseNumberOrName { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string County { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public string MerchantSig { get; set; }

        public BillingAddress()
        {
            Country = "GB";
            Type = "";
        }

        public Dictionary<string, string> SigningParams => new()
        {
            { "billingAddress.street", Street },
            { "billingAddress.houseNumberOrName", HouseNumberOrName },
            { "billingAddress.city", City },
            { "billingAddress.postalCode", PostCode },
            { "billingAddress.stateOrProvince", County },
            { "billingAddress.country", Country },
        };

        public void CreateMerchantSignature(string hmacKey)
        {
            MerchantSig = HmacUtilities.CalculateHmac(hmacKey, SigningUtilities.BuildSigningString(SigningParams));
        }
    }
}
