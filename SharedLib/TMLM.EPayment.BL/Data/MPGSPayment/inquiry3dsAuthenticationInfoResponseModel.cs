using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class Inquiry3dsAuthenticationInfoRequestModel
    {
        public string orderNo { get; set; }
    }
    public class Inquiry3dsAuthenticationInfoResponseModel
    {
        public bool isSuccess { get; set; }
        public string errorMessage { get; set; }
    }
}
