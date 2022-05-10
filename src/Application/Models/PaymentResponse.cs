using System;

namespace Application.Models 
{ 
    public class PaymentResponse
    {
        public decimal Auth_Amount { get; set; }
        public int Auth_Response { get; set; }
        public string Transaction_Id { get; set; }
        public string Req_Currency { get; set; }
        public string Decision { get; set; }
        public string Signed_Field_Names { get; set; }
        public string Req_Reference_Number { get; set; }
        public string Message { get; set; }
        public string Signature { get; set; }
        public int Reason_Code { get; set; }
        public DateTime Auth_Time { get; set; }

        public string Req_Payment_Method { get; set; }

        public override string ToString()
        {
            return $"{nameof(Auth_Amount)}: {Auth_Amount}, {nameof(Auth_Response)}: {Auth_Response}, {nameof(Transaction_Id)}: {Transaction_Id}, {nameof(Req_Currency)}: {Req_Currency}, {nameof(Decision)}: {Decision}, {nameof(Signed_Field_Names)}: {Signed_Field_Names}, {nameof(Req_Reference_Number)}: {Req_Reference_Number}, {nameof(Message)}: {Message}, {nameof(Signature)}: {Signature}, {nameof(Reason_Code)}: {Reason_Code}, {nameof(Auth_Time)}: {Auth_Time}, {nameof(Req_Payment_Method)}: {Req_Payment_Method}";
        }
    }

}
