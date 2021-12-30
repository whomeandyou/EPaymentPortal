using log4net;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider.FPX;
using TMLM.EPayment.BL.Service;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Service.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.WebApi.ViewModels;

namespace TMLM.EPayment.WebApi.Areas.Internal.Controllers
{
    public class PaymentController : BaseController
    {
        [HttpPost, ActionName("InitiatePayment")]
        public ActionResult InitiatePayment(InitiatePaymentInputModel model)
        {
            var ppFactory = new PaymentProvicerFactory();
            var processor = ppFactory.GetPaymentProcessor((PaymentProviderType)model.PaymentProviderId);

            return Json(processor.InitiatePayment(model));
        }

        [HttpPost, ActionName("GenerateHtml")]
        public ActionResult GenerateHtml(GetHtmlInputModel model)
        {
            try
            {
                var paymentProviderType = GetPaymentProviderTypeByTransactionNumber(model.TransactionNumber);

                var ppFactory = new PaymentProvicerFactory();
                var processor = ppFactory.GetPaymentProcessor(paymentProviderType);

                return Json(processor.GenerateRequestHTML(model));
            }
            catch (Exception ex)
            {
                return Json(new GetHtmlOutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("GenerateMandateHtml")]
        public ActionResult GenerateMandateHtml(GetHtmlInputModel model)
        {
            try
            {
                var paymentProviderType = GetEMandateByTransactionNumber(model.TransactionNumber);
                var ppFactory = new PaymentProvicerFactory();
                var processor = ppFactory.GetPaymentProcessor(paymentProviderType);

                return Json(processor.GenerateRequestHTML(model));
            }
            catch (Exception ex)
            {
                return Json(new GetHtmlOutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("ProcessPayment")]
        public ActionResult ProcessPayment(ProcessPaymentInputModel model)
        {
            var ppFactory = new PaymentProvicerFactory();
            var processor = ppFactory.GetPaymentProcessor((PaymentProviderType)model.PaymentProviderId);

            return Json(processor.ProcessPayment(model));
        }

        [HttpPost, ActionName("CancelTrans")]
        public ActionResult CancelPayment(string transactionNumber)
        {
            try
            {
                var paymentProviderType = GetPaymentProviderTypeByTransactionNumber(transactionNumber);

                var ppFactory = new PaymentProvicerFactory();
                var processor = ppFactory.GetPaymentProcessor(paymentProviderType);

                return Json(processor.CancelPayment(transactionNumber));
            }
            catch (Exception ex)
            {
                return Json(new GetHtmlOutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("FailWithStatus")]
        public ActionResult FailPaymentWithStatus(string transactionNumber, string status)
        {
            try
            {
                var paymentProviderType = GetPaymentProviderTypeByTransactionNumber(transactionNumber);

                var ppFactory = new PaymentProvicerFactory();
                var processor = ppFactory.GetPaymentProcessor(paymentProviderType);

                return Json(processor.FailPaymentWithStatus(transactionNumber, status));
            }
            catch (Exception ex)
            {
                return Json(new GetHtmlOutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("UpdateInfo")]
        public ActionResult UpdateInfo(UpdatePaymentInformationInputModel model)
        {
            try
            {
                using (var svcPaymentTransaction = new PaymentTransactionService())
                    svcPaymentTransaction.UpdatePaymentInformation(model);

                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_Success
                });
            }
            catch (Exception ex)
            {
                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("UpdateEMandateInfo")]
        public ActionResult UpdateEMandateInfo(UpdatePaymentInformationInputModel model)
        {
            try
            {
                using (var svcPaymentTransaction = new EMandateTransactionService())
                    svcPaymentTransaction.UpdateEMandateInformation(model);

                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_Success
                });
            }
            catch (Exception ex)
            {
                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("GetPaymentInfo")]
        public ActionResult GetPaymentInfo(string orderNum,string transactionNum)
        {
            try
            {
                using (var svcPaymentTransaction = new PaymentTransactionService())
                    svcPaymentTransaction.GetPaymentInformationByOrderIdAndTransactioNum(orderNum,transactionNum);

                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_Success
                });
            }
            catch (Exception ex)
            {
                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        [HttpPost, ActionName("InquiryPayment")]
        public ActionResult InquiryPayment(InquiryPaymentInputModel model)
        {
            try
            {
                var ppFactory = new PaymentProvicerFactory();
                var processor = ppFactory.GetPaymentProcessor((PaymentProviderType)model.PaymentProviderType);


                return Json(processor.Inquiry(model));
            }
            catch (Exception ex)
            {
                return Json(new OutputModel()
                {
                    Code = ResponseReturnCode.Gen_InternalServerError,
                    Message = ex.Message
                });
            }
        }

        #region Utilities 

        private PaymentProviderType GetPaymentProviderTypeByTransactionNumber(string transactionNumber)
        {
            var paymentProviderCode = string.Empty;

            using (var svcPaymentTransaction = new PaymentTransactionService())
            {
                var paymentTransaction = svcPaymentTransaction.GetPaymentTransactionByTransactionNumber(transactionNumber);

                if (paymentTransaction == null)
                    throw new Exception("Invalid transactionNumber");

                paymentProviderCode = paymentTransaction.PaymentProviderCode;
            }
            return (PaymentProviderType)Enum.Parse(typeof(PaymentProviderType), paymentProviderCode);
        }

        private PaymentProviderType GetEMandateByTransactionNumber(string transactionNumber)
        {

            using (var svcPaymentTransaction = new EMandateTransactionService())
            {
                var paymentTransaction = svcPaymentTransaction.GetPaymentTransactionByTransactionNumber(transactionNumber);

                if (paymentTransaction == null)
                    throw new Exception("Invalid transactionNumber");
            }
            return (PaymentProviderType)Enum.Parse(typeof(PaymentProviderType), "EMandate");
        }

        #endregion
    }
}