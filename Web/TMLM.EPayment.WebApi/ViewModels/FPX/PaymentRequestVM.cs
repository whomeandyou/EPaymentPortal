
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.WebApi.ViewModels.FPX
{
    public class PaymentRequestVM
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

        public string PaymentRef { get; set; }

        public int Mode { get; set; }
    }
}
