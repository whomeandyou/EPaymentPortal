using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TMLM.Common;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.PaymentProvider;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class CommonPaymentController : Controller
    {

        //[HttpPost, Route("Payment/Selection")]
        //public ActionResult PaymentSelection(PaymentRequestVM model)
        //{

        //}

        [HttpPost, Route("Payment/Inquiry")]
        public JsonResult InquiryPayment(InquiryPaymentInputModel model)
        {
 

            var ppFactory = new PaymentProvicerFactory();
            var processor = ppFactory.GetPaymentProcessor((PaymentProviderType)model.PaymentProviderType);

            return Json(processor.Inquiry(model), JsonRequestBehavior.AllowGet);
        }

        private PaymentProviderType GetPaymentProviderTypeByOrderNumber(string orderNumber,string paymentProviderType)
        {
            var paymentProviderCode = string.Empty;

            using (var svcPaymentTransaction = new PaymentTransactionService())
            {
                var paymentTransaction = svcPaymentTransaction.GetPaymentTransactionByOrderNumber(orderNumber, paymentProviderType);

                if (paymentTransaction == null)
                    throw new Exception("Invalid transactionNumber");

                paymentProviderCode = paymentTransaction.PaymentProviderCode;
            }
            return (PaymentProviderType)Enum.Parse(typeof(PaymentProviderType), paymentProviderCode);
        }
    }
}