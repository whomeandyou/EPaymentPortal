
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using TMLM.EPayment.BL.PaymentProvider;
using System.Collections.Specialized;

namespace TMLM.EPayment.BL.Data.PaymentProvider
{
    public class UpdatePaymentInformationInputModel
    {
        [DataMember]
        public string MsgToken { get; set; }
        [DataMember]
        public string TransactionNumber { get; set; }

        [DataMember]
        public string BuyerEmail { get; set; }

        [DataMember]
        public string BuyerBank { get; set; }

        [DataMember]
        public string SecureId { get; set; }

        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public string ProposalId { get; set; }

        [DataMember]
        public bool IsDifferentRenewalMethod { get; set; }

        [DataMember]
        public int Status { get; set; }
    }
}
