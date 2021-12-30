using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.RazerPay
{
    public class RazerPayDecryptOutputModel
    {
        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string MerchantId { get; set; }
    }
}
