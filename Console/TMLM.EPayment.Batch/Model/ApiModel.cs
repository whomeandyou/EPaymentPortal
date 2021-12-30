using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class ApiModel
    {
        public string BaseUrl { get; set;}
        public string Password { get; set; }
        public string Version { get; set; }
        public Encoding EncodingType { get; set; }
        public string ContentType { get; set; }
    }
}
