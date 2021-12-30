using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch
{
    public static class ApiStatus
    {
        public static string Complete = "COMPLETE";
        public static string Upload = "UPLOAD";
        public static string Fail = "FAILED";
        public static string MicValidation = "MIC VALIDATION";
    }
}
