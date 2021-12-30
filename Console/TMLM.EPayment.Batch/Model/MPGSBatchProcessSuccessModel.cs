using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class MPGSBatchProcessSuccessModel
    {
        public string PolicyNumber { get; set; }
        public string Result { get; set; }
        public string AgreementId { get; set; }
    }
}
