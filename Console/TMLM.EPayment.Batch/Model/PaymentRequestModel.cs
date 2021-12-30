using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class PaymentRequestModel
    {
        public int Id;
        public int BatchId;
        public string Status;
        public string RequestUrl;
        public string RequestBody;
        public string ResponseBody;

        public DateTime CreatedDate;
        public DateTime LastUpdated;
    }
}
