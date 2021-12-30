using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class PaymentResponseInputModel
    {
        public string Amount { get; set; }

        public string Orderid { get; set; }

        public string TranID { get; set; }

        public string Domain { get; set; }

        public string Status { get; set; }

        public string appcode { get; set; }

        public string Error_code { get; set; }

        public string Error_desc { get; set; }

        public string Skey { get; set; }

        public string Currency { get; set; }

        public string Channel { get; set; }

        public string Paydate { get; set; }

        public string ExtraP { get; set; }
    }
}
