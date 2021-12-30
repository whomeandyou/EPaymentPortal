using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class ResponseToMerchant
    {
        public bool IsSuccess { get; set; }
        public string OrderNo { get; set; }
        public string AppId { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
        public string BankName { get; set; }
        public string CCNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CardType { get; set; }
        public string CardMethod { get; set; }
        public string ReturnURL { get; set; }
        public string AmountPaid { get; set; }
        public string TransCode { get; set; }
        public string AuthCode { get; set; }
        public string IsEnrolment { get; set; }

        public string IsInitialPayment { get; set; }
        public bool IsDifferentRenewalMethod { get; set; }

        public string msgSign { get; set; }

        public string Veres { get; set; }
        public string Pares { get; set; }
        public string AcsEci { get; set; }
        public string AuthenticationToken { get; set; }
        public string TransactionId { get; set; }
        public string newTransactionId { get; set; }
        public string dsVersion {get;set;}

        public DateTime? PaymentDate { get; set; }
        public string PaymentRef { get; set; }
    }
}
