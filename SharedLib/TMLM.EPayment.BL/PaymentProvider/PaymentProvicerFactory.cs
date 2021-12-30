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
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider.FPX;
using TMLM.EPayment.BL.PaymentProvider.MPGS;
using TMLM.EPayment.BL.PaymentProvider.RazerPay;

namespace TMLM.EPayment.BL.Service.PaymentProvider
{
    public class PaymentProvicerFactory
    {
        public IPaymentProcessor GetPaymentProcessor(PaymentProviderType paymentProviderType)
        {
            switch (paymentProviderType)
            {
                case PaymentProviderType.EMandate:
                    return new EMandateProcessor();
                case PaymentProviderType.FPX:
                    return new FPXProcessor();
                case PaymentProviderType.MPGS:
                    return new MPGSProcessor();
                case PaymentProviderType.RazerPay:
                    return new RazerPayProcessor();

            }

            throw new NotImplementedException("Not Implemented");
        }
    }
}
