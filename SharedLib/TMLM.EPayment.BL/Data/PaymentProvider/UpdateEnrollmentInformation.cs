using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class EnrollmentInformationModel
    {
        public int PaymentTransactionId { get; set; }
        public string Veres { get; set; }
        public string Pares { get; set; }
        public string AcsEci { get; set; }
        public string AuthenticationToken { get; set; }
        public string TransactionId { get; set; }
        public string dsVersion { get; set; }
        public string OrderNumber { get; set; }
        public string TransactionStatus { get; set; }
        public string StatusReasonCode { get; set; }
        public string CreditCardNumber { get; set; }
        public string ReceiveTransactionID { get; set; }
        
    }
}
