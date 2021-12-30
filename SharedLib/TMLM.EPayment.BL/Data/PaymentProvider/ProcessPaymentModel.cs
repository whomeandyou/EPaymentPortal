
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using TMLM.EPayment.BL.PaymentProvider;
using System.Collections.Specialized;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class ProcessPaymentInputModel
    {
        [DataMember]
        public int PaymentProviderId { get; set; }

        [DataMember]
        public IDictionary<string, string> Form { get; set; }

    }

    public class ProcessPaymentOutputModel : OutputModel
    {
        [DataMember]
        public PaymentProviderType PaymentProviderType { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public string ReturnUrl { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public string MerchantId { get; set; }

        [DataMember]
        public string PaymentResponseCode { get; set; }

        [DataMember]
        public string ApplicationAccountCode { get; set; }

        [DataMember]
        public string Bank { get; set; }

        [DataMember]
        public string ReferenceNumber { get; set; }

        [DataMember]
        public string Payload { get; set; }

        [DataMember]
        public string TransactionNumber { get; set; }

        [DataMember]
        public string OrderNumber { get; set; }

        [DataMember]
        public string AuthorizationCode { get; set; }

        [DataMember]
        public string AuthorizationNumber { get; set; }

        [DataMember]
        public string BuyerEmail { get; set; }

        [DataMember]
        public string CardMethod { get; set; }

        [DataMember]
        public string CardType { get; set; }

        [DataMember]
        public string CardNumber { get; set; }

        [DataMember]
        public string ExpiryMonth { get; set; }

        [DataMember]
        public string ExpiryYear { get; set; }

        [DataMember]
        public int Mode { get; set; }
        [DataMember]
        public string PaymentRef { get; set; }
        [DataMember]
        public string ApplicationType { get; set; }
        [DataMember]
        public int MaxFrequency { get; set; }
        [DataMember]
        public string FrequencyMode { get; set; }
        [DataMember]
        public string ProductDesc { get; set; }
        [DataMember]
        public decimal DirectDebitAmount { get; set; }
        [DataMember]
        public string LAFPXBankKey { get; set; }
        [DataMember]
        public string LAFPXBankName { get; set; }

    }
}
