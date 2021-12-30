
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db.Tables
{
    public class PaymentTransaction : BaseTable
    {
        public override System.Reflection.PropertyInfo[] TableColumns
        {
            get
            {
                return this.GetType().GetProperties().Where(
                    prop => System.Attribute.IsDefined(prop, typeof(TableColumnAttribute))).ToArray();
            }
        }

        [TableColumn]
        public int Id { get; set; }
        [TableColumn]
        public string TransactionNumber { get; set; }
        [TableColumn]
        public string MsgToken { get; set; }
        [TableColumn]
        public string PaymentProviderCode { get; set; }
        [TableColumn]
        public string AuthorizationCode { get; set; }
        [TableColumn]
        public string AuthorizationNumber { get; set; }
        [TableColumn]
        public string ResponsePayload { get; set; }
        [TableColumn]
        public string OrderNumber { get; set; }
        [TableColumn]
        public string MerchantId { get; set; }
        [TableColumn]
        public decimal Amount { get; set; }
        [TableColumn]
        public int TransactionStatusId { get; set; }
        [TableColumn]
        public DateTime CreatedOn { get; set; }
        [TableColumn]
        public DateTime? LastModifiedOn { get; set; }
        [TableColumn]
        public int ApplicationAccountId { get; set; }
        [TableColumn]
        public string ReturnUrl { get; set; }
        [TableColumn]
        public string Bank { get; set; }
        [TableColumn]
        public string BuyerEmail { get; set; }
        [TableColumn]
        public string BuyerName { get; set; }
        [TableColumn]
        public string BuyerContact { get; set; }
        [TableColumn]
        public string Description { get; set; }
        [TableColumn]
        public string Channel { get; set; }
        [TableColumn]
        public string CancelUrl { get; set; }
        [TableColumn]
        public string PaymentReferenceNumber { get; set; }
        [TableColumn]
        public string CardNumber { get; set; }
        [TableColumn]
        public string ExpiryMonth { get; set; }
        [TableColumn]
        public string ExpiryYear { get; set; }
        [TableColumn]
        public string SecurityCode { get; set; }
        [TableColumn]
        public string Currency { get; set; }
        [TableColumn]
        public string AdditionalInfo { get; set; }
        [TableColumn]
        public string SessionId { get; set; }
        [TableColumn]
        public string SecureId { get; set; }
        [TableColumn]
        public string ProposalId { get; set; }
        [TableColumn]
        public string ResponseMessage { get; set; }
        [TableColumn]
        public string ResponseCode { get; set; }
        [TableColumn]
        public string ErrorMessage { get; set; }
        [TableColumn]
        public string CardType { get; set; }
        [TableColumn]
        public string CardMethod { get; set; }
        [TableColumn]
        public string AppId { get; set; }
        [TableColumn]
        public string TransCode { get; set; }
        [TableColumn]
        public bool IsEnrolment { get; set; }
        [TableColumn]
        public bool IsInitialPayment { get; set; }
        [TableColumn]
        public bool IsDifferentRenewalMethod { get; set; }
        [TableColumn]
        public int Mode { get; set; }
        [TableColumn]
        public string ApplicationAccountCode { get; set; }
        [TableColumn]
        public string NewTransactionId { get; set; }
    }

    public class EnrollmentStatus : BaseTable
    {
        public override System.Reflection.PropertyInfo[] TableColumns
        {
            get
            {
                return this.GetType().GetProperties().Where(
                    prop => System.Attribute.IsDefined(prop, typeof(TableColumnAttribute))).ToArray();
            }
        }

        [TableColumn]
        public int Id { get; set; }
        [TableColumn]
        public int PaymentTransactionId { get; set; }
        [TableColumn]
        public string Veres { get; set; }
        [TableColumn]
        public string Pares { get; set; }
        [TableColumn]
        public DateTime CreatedOn { get; set; }
        [TableColumn]
        public DateTime LastModifiedOn { get; set; }
        [TableColumn]
        public string AcsEci { get; set; }
        [TableColumn]
        public string AuthenticationToken { get; set; }
        [TableColumn]
        public string TransactionId { get; set; }
        [TableColumn]
        public string dsVersion { get; set; }
        [TableColumn]
        public string OrderNumber { get; set; }
        [TableColumn]
        public string TransactionStatus { get; set; }
        [TableColumn]
        public string StatusReasonCode { get; set; }
        [TableColumn]
        public string CreditCardNumber { get; set; }
        [TableColumn]
        public string ReceiveTransactionID { get; set; }

    }
}
