using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class RazerPayInquiryResponse
    {
        public string Amount { get; set; }
        public string TranID { get; set; }
        public string Domain { get; set; }
        public string Channel { get; set; }
        public string VrfKey { get; set; }
        public string StatCode { get; set; }
        public string StatName { get; set; }
        public string Currency { get; set; }
        public string OrderID { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string ErrorMessage { get; set; }
    }
}
