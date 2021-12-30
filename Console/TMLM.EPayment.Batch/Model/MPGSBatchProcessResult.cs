using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class MPGSBatchProcessResult
    {
        public int ProcessID { get; set; }
        public int BatchID { get; set; }
        public string MerchantID { get; set; }
        public string RecordType { get; set; }
        public string MechantNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PolicyNumber { get; set; }
        public string PostCode { get; set; }
        public decimal SumAssured { get; set; }
        public string OrderID { get; set; }
        public string TransactionID { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string BankCard { get; set; }
        public string BankExpiry { get; set; }
        public string Result { get; set; }
        public string ErrorCause { get; set; }
        public string ErrorDescription { get; set; }
        public string GatewayCode { get; set; }
        public string ApiOperation { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; }
        public string AuthorizationCode { get; set; }
    }
}
