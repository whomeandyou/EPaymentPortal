using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class MPGSResponseModel
    {
        //result	error.cause	error.explanation	response.gatewayCode

        public string ApiOperation { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string OrderRef { get; set; }
        public string TransactionRef { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string BankNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Result { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string GatewayCode { get; set; }
        public string GatewayDesc { get; set; }
        public string AuthorizationCode { get; set; }
        public string Source { get; set; }
        public bool IsUpdated { get; set; }
        public string dsTransactionId { get; set; }
        public string dsAuthenticationToken { get; set; }
        public string dsAcsEci { get; set; }
        public string ds2TransactionStatus { get; set; }
        public string Veres { get; set; }
        public string Pares { get; set; }
        public string CardBrand { get; set; }
        public string CardMethod { get; set; }
    }
}
