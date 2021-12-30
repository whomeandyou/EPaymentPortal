using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TMLM.EPayment.BL.Data.PaymentProvider;

namespace TMLM.EPayment.BL.PaymentProvider
{
    public enum PaymentProviderType
    {
        FPX = 1,
        MPGS = 2,
        EMandate = 3,
        RazerPay = 4
    }
}
