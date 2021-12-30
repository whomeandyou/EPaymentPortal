using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class GetHtmlInputModel
    {
        public string TransactionNumber { get; set; }
        public string TransactionType { get; set; }
        public string BuyerBank { get; set; }
        public string BuyerEmail { get; set; }
    }

    public class GetHtmlOutputModel : OutputModel
    {
        public string FormHtml { get; set; }
    }
}
