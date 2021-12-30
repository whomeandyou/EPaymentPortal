using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class SummaryInfo
    {
        public string CardType { get; set; }
        public string CardMethod { get; set; }
        public string GatewayCode { get; set; }
        public decimal Amount { get; set; }
        public int Total { get; set; }
    }
}
