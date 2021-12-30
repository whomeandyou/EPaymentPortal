using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class InquiryRequestInputModel
    {
        public string OrderNo { get; set; }

        public string ApplicationAccountCode { get; set; }

        public string InquiryUrl { get; set; }

        public string Amount { get; set; }

        public string MsgSign { get; set; }
    }
}
