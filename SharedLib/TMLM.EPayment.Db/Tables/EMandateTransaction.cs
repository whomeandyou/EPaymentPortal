using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db.Tables
{
    public class EMandateTransaction : BaseTable
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
        public string BuyerEmail { get; set; }
        [TableColumn]
        public string OrderNumber { get; set; }
        [TableColumn]
        public decimal Amount { get; set; }
        [TableColumn]
        public DateTime CreatedOn { get; set; }
        [TableColumn]
        public DateTime? LastModifiedOn { get; set; }
        [TableColumn]
        public int ApplicationAccountId { get; set; }
        [TableColumn]
        public string PaymentReferenceNumber { get; set; }
        [TableColumn]
        public string ReturnUrl { get; set; }
        [TableColumn]
        public string Bank { get; set; }
        [TableColumn]
        public string ResponsePayload { get; set; }
        [TableColumn]
        public string AuthorizationCode { get; set; }
        [TableColumn]
        public string AuthorizationNumber { get; set; }
        [TableColumn]
        public string MerchantId { get; set; }
        [TableColumn]
        public string AdditionalInfo { get; set; }
        [TableColumn]
        public string Currency { get; set; }
        [TableColumn]
        public string IdType { get; set; }
        [TableColumn]
        public string IdNo { get; set; }
        [TableColumn]
        public string ApplicationType { get; set; }
        [TableColumn]
        public int MaxFrequency { get; set; }
        [TableColumn]
        public string FrequencyMode { get; set; }
        [TableColumn]
        public DateTime? ExpiryDate { get; set; }
        [TableColumn]
        public string PhoneNo { get; set; }
        [TableColumn]
        public int TransactionStatusId { get; set; }
        [TableColumn]
        public string MsgToken { get; set; }
        [TableColumn]
        public int Mode { get; set; }
        [TableColumn]
        public string PaymentRef { get; set; }
        [TableColumn]
        public string Descriptions { get; set; }

    }
}
