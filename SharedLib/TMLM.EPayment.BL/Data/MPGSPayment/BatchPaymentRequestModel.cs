using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class BatchPaymentRequestModel
    {
        public string path { get; set; }
        public string method { get; set; }
        public string body { get; set; }
        public string AuthorizePassword { get; set; }
        public string EncodingType { get; set; }
        public string ContentType { get; set; }
    }
}
