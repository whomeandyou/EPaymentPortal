using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class PaymentRequestInputModel
    {
        public string Amount { get; set; }

        public string OrderNo { get; set; }

        public string BuyerName { get; set; }

        public string BuyerEmail { get; set; }

        public string BuyerContact { get; set; }

        public string Description { get; set; }

        public string Country { get; set; }

        public string MsgSign { get; set; }

        public string AId { get; set; }

        public string ReturnUrl { get; set; }

        public string CancelUrl { get; set; }
    }
}
