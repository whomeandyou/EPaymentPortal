using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class GetPaymentInforOutputModel : OutputModel
    {
        public int Id;
        public string CreatedOn { get; set; }
        public string OrderNo { get; set; }
        public string Amt { get; set; }
        public string AId { get; set; }
        public string Status { get; set; }
        public string MsgSign { get; set; }
        public string Bank { get; set; }
        public string RefNo { get; set; }
        public string AuthCode { get; set; }
        public string AuthNo { get; set; }
        public string SellerId { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CardMethod { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public bool IsDifferentRenewalMethod { get; set; }
        public string IsInitialPayment { get; set; }
        public string IsEnrolment { get; set; }
        public string TransactionCode { get; set; }
        public string ApplicationId { get; set; }
        public int ApplicationAccountId { get; set; }
        public string SessionID { get; set; }

        public string SecureID { get; set; }
        public string Currency { get; set; }
        public string returnURL { get; set; }
        public string proposalID { get; set; }
        public string ApplicationAccountCode { get; set; }
    }
}
