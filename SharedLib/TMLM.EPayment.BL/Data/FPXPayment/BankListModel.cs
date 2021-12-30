using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.BL.Data;

namespace TMLM.EPayment.BL.Data.FPXPayment
{
    public class GetBankListOutputModel : OutputModel
    {
        [DataMember]
        public List<GetBankList> BankList { get; set; }
    }

    public class GetBankList
    {
        [DataMember]
        public string BankCode { get; set; }

        [DataMember]
        public string BankName { get; set; }
    }
}
