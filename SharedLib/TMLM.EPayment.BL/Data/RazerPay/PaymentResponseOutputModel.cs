using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class PaymentResponseOutputModel
    {
        public string Amount { get; set; }

        public string RazerPayTransactionNo { get; set; }

        public string OrderNo { get; set; }

        public string Domain { get; set; }

        public string Status { get; set; }
        
        public string AppCode { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDescription { get; set; }

        public string SKey { get; set; }

        public string Currency { get; set; }

        public string Channel { get; set; }

        public string PayDate { get; set; }

        public string ExtraParam { get; set; }

        public string MsgSign { get; set; }
    }
}
