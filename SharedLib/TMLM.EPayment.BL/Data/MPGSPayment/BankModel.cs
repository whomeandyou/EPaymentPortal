using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class  GetBankNameRequestModel
    {
        public string CCNumber { get; set; }
        
    }
    public class GetBankNameResponseModel
    {
        public string bankName { get; set; }
        public string bin { get; set; }

    }
}
