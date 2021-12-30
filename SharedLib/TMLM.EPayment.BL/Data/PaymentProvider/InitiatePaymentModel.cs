using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class InitiatePaymentInputModel
    {
        public DateTime CreatedOn { get; set; }
        public string Currency { get; set; }
        public string TransactionNumber { get; set; }
        public string OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public int PaymentProviderId { get; set; }
        public string ApplicationAccountCode { get; set; }
        public string ReturnUrl { get; set; }
        public string AdditionalInfo { get; set; }
        public bool IsEnrolment { get; set; }
        public bool IsInitialPayment { get; set; }
        public string BuyerEmail { get; set; }
        public string IdType { get; set; }
        public string IdNo { get; set; }
        public string ApplicationType { get; set; }
        public int MaxFrequency { get; set; }
        public string FrequencyMode { get; set; }
        public string PayorName { get; set; }
        public string PurposeOfPayment { get; set; }
        public string MobilePhoneNo { get; set; }
        public string PaymentRef { get; set; }
        public string MsgToken { get; set; }
        public int Mode { get; set; }
        public string BuyerName { get; set; }
        public string Description { get; set; }
        public string CancelUrl { get; set; }

    }
}
