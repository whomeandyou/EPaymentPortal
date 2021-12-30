using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class ExtractDataModel
    {
        // Extract Data
        /// Merchant Id - 4
        /// Recod type - 1
        /// Merchant Number - 9
        /// Policy Number - 25 (Trim whitespaces)
        /// Account Number - 16
        /// Expiry Date - 4
        /// Amount - 11
        /// Account Name - 26
        /// Address - 60
        /// Phone Number - 10
        /// Post Code - 5
        /// Sum Assured - 12

        public int ProcessID { get; set; }
        public int BatchId { get; set; }
        public string MerchantId { get; set; }
        public string RecordType { get; set; }
        public string MerchantNumber { get; set; }
        public string PolicyNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string ExpiryDate { get; set; }
        public string Amount { get; set; }
        public string AccountName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string PostCode { get; set; }
        public string SumAssured { get; set; }
        public string ApiOperation { get; set; }
        public string Currency { get; set; }
        public string PaymentType { get; set; }
        public string AgreementId { get; set; }
        public string dsTransactionId { get; set; }
        public string dsAuthenticationToken { get; set; }
        public string dsAcsEci { get; set; }
        public string dsVersion { get; set; }
        public string Veres { get; set; }
        public string Pares { get; set; }
        public string GatewayCode { get; set; }
        public string Result { get; set; }
        public bool HasSucessTransactedRecord { get; set; }
        public string OrderId { get; set; }
        public string ResponseData { get; set; }
        public string AuthorizationCode { get; set; }
        public string CardBrand { get; set; }
        public string CardMethod { get; set; }
    }
}
