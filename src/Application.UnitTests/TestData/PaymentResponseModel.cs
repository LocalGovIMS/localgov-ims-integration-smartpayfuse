using Application.Models;
using System;

namespace Application.UnitTests
{
    public partial class TestData
    {
        public static PaymentResponse GetPaymentResponseModel()
        {
            return new PaymentResponse()
            {
                Auth_Amount = 1.00M,
                Auth_Response = 0,
                Transaction_Id = "123456789",
                Req_Currency = "GB",
                Decision = "ERROR",
                Signed_Field_Names = "transaction_id",
                Req_Reference_Number = "",
                Message = "",
                Signature = "",
                Reason_Code = 0,
                Auth_Time = DateTime.Now,
                Req_Payment_Method = ""
            };
        }
    }
}