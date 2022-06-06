using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Models
{
    public class SmartPayFusePayment
    {
        /// <summary>
        /// Required for authentication with
        /// Secure Acceptance. Generated in
        /// EBC Secure Acceptance profile.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Decimal amount e.g. 20.00 is £20.00
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Flag that specifies the purpose of
        /// the authorization for MasterCard
        ///    cards. Possible values:
        /// 0: Preauthorization
        /// 1: Final authorization
        /// </summary>
        public int AuthIndicator { get; set; } = 1;

        public string BillToForename { get; set; }
        public string BillToSurname { get; set; }
        public string BillToEmail { get; set; } = "noreply@barnsley.gov.uk"; //TODO: set in config
        public string BillToAddressLine1 { get; set; }
        public string BillToAddressCity { get; set; }
        public string BillToAddressState { get; set; }
        public string BillToAddressCountry { get; set; } = "GB";
        public string BillToAddressPostalCode { get; set; }

        /// <summary>
        /// Currency for the transaction, defaults to 'GBP'
        /// </summary>
        public string Currency { get; set; } = "GBP";

        /// <summary>
        /// Locale for the transaction, defaults to 'en'
        /// </summary>
        public string Locale { get; set; } = "en";

        /// <summary>
        /// Ignore the results of AVS
        /// verification.
        /// </summary>
        public bool IgnoreAvs { get; set; } = false;

        /// <summary>
        /// Ignore the results of CVN
        /// verification
        /// </summary>
        public bool IgnoreCvn { get; set; } = false;

        /// <summary>
        /// Total number of line items.
        /// Used when order has items and
        /// not just Grand Total Amount
        /// value
        /// </summary>
        public int LineItemCount { get; set; } = 1;

        /// <summary>
        /// Overrides the backoffice post URL
        /// profile setting with merchant
        /// URL. URL must be HTTPS and
        /// support TLS 1.2 or later.
        /// </summary>
        public string OverrideBackofficePostUrl { get; set; }

        /// <summary>
        /// Overrides the custom cancel page
        /// profile setting with merchant
        /// URL. URL must be HTTPS and
        /// support TLS 1.2 or later.
        /// </summary>
        public string OverrideCustomCancelPage { get; set; }

        /// <summary>
        /// Overrides the custom receipt
        /// profile setting with merchant
        /// URL. URL must be HTTPS and
        /// support TLS 1.2 or later.
        /// </summary>
        public string OverrideCustomReceiptPage { get; set; }

        /// <summary>
        /// Profile ID from the SmartPay Fuse merchant account
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        /// Unique merchant generated
        /// order reference or tracking number for
        /// each transaction
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// HMAC256 Transaction signature 
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Date the transactions has been signed
        /// </summary>
        public DateTime SignedDateTime { get; set; }

        /// <summary>
        /// List of field names used to sign the request
        /// </summary>
        public List<string> SignedFieldNames { get; set; }

        /// <summary>
        /// Define type of SA transaction This field may have the following values:
        /// - “authorization”
        /// - “authorization,create_payment_token”
        /// - “authorization,update_payment_token”
        /// - “sale”
        /// - “sale,create_payment_token”
        /// - “sale,update_payment_token”
        /// - “create_payment_token”
        /// - “update_payment_token”
        /// Decision Manager and Payer Authentication
        ///     services will be configured in SA profile.
        /// </summary>
        public string TransactionType { get; set; } = "sale";

        /// <summary>
        /// Unique merchant-generated
        /// identifier. This field is used to
        /// check for duplicate transaction
        /// attempts.
        /// </summary>
        public Guid TransactionUuid { get; set; } = Guid.NewGuid();

        public string SecretKey { get; set; }

        public string SmartPayFusePaymentEndpoint { get; set; }

    }
}
