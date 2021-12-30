using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMLM.EPayment.WebApi.ViewModels.FPX
{
    public class EMandateRequestVM
    {
        /// <summary>
        /// Return Url
        /// </summary>
        public string RetUrl { get; set; }

        /// <summary>
        /// Order Number
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public string Amt { get; set; }

        /// <summary> {
        /// 
        /// Application Account Id
        /// </summary>
        public string AId { get; set; }

        /// <summary>
        /// Message Signature
        /// </summary>
        public string MsgSign { get; set; }

        public string IdType { get; set; }
        public string IDNo { get; set; }
        public string ApplicationType { get; set; } = "01"; // 01 - new mandate
        public string MobilePhoneNo { get; set; }
        public int MaxFrequency { get; set; } = 2;
        public string FreqMode { get; set; } = "MT"; // monthly
        public DateTime EffectiveDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; }
        public string BuyerEmail { get; set; } = "";
        public string PurposeOfPayment { get; set; }
        public string Name { get; set; }
        public string PaymentRef { get; set; }
        public string MsgToken { get; set; } = "01"; // 01 - b2c, 02 - b2b1
        public int Mode { get; set; }
    }
}