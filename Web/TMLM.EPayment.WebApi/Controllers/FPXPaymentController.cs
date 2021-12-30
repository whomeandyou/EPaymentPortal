using log4net;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Service;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.WebApi.ViewModels;
using TMLM.EPayment.BL.PaymentProvider.FPX;
using TMLM.EPayment.WebApi.ViewModels.FPX;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class FPXPaymentController : Controller
    {
        [HttpPost, Route("Payment/Request")]
        public ActionResult RequestView(PaymentRequestVM model)
        {
            //for debugging purpose in production.
            LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(model));

            //check signature
            var hashAuthentication = new HashAuthentication();
            if (!hashAuthentication.CompareRequestSignature(model.Amt, model.OrderNo, model.AId,
                model.MsgSign))
            {
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Invalid Signature"
                });
            }


            var svcPayment = new PaymentService();
            InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
            {
                OrderNo = model.OrderNo,
                PaymentProviderType = (int)PaymentProviderType.FPX,
                AId = model.AId,
                MsgSign = model.MsgSign,
                Amt = model.Amt
            };
            var inquiryResult = svcPayment.InquiryPayment(inquiryPaymentInputModel);
            if (inquiryResult.Status == PaymentResponseCode.Success.ToString())
            {
                var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", inquiryResult.Amt), inquiryResult.OrderNo,
                    int.Parse(inquiryResult.Status), inquiryResult.ApplicationAccountId.ToString(),string.Empty);

                
                var viewModelPaymentResultInquiry = new PaymentResultVM()
                {
                    MsgSign = msgSign,
                    AId = model.AId,
                    Amt = string.Format("{0:.00}", inquiryResult.Amt),
                    ReturnUrl = model.RetUrl,
                    Status = inquiryResult.Status,
                    OrderNo = model.OrderNo,
                    Bank = inquiryResult.Bank != null ? inquiryResult.Bank : string.Empty,
                    BankName = inquiryResult.BankName != null ? inquiryResult.BankName : string.Empty,
                    RefNo = inquiryResult.RefNo,
                    AuthCode = inquiryResult.AuthCode,
                    AuthNo = inquiryResult.AuthNo,
                    SellerId = inquiryResult.SellerId,
                    CreatedOn = inquiryResult.CreatedOn,
                    IsEMandate = false,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef,
                    LABankKey = inquiryResult.LABankKey,
                    LABankName = inquiryResult.LABankName
                };

                return View("~/Views/Payment/Result.cshtml", viewModelPaymentResultInquiry);
            }


            var transactionNumber = Guid.NewGuid().ToString();
           
            var initiateResult = svcPayment.InitiatePayment(new InitiatePaymentInputModel()
            {
                ApplicationAccountCode = model.AId,
                Amount = Convert.ToDecimal(model.Amt),
                PaymentProviderId = (int)PaymentProviderType.FPX,
                TransactionNumber = transactionNumber,
                OrderNumber = model.OrderNo,
                CreatedOn = DateTime.Now,
                ReturnUrl = model.RetUrl,
                Mode = model.Mode,
                PaymentRef = model.PaymentRef
            });

            if (initiateResult.Code != ResponseReturnCode.Gen_Success)
            {
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Failed to initiate payment. " + initiateResult.Message
                });
            }

            var viewModel = new PaymentSummaryVM()
            {
                MerchantName = "Tokio Marine Life Insurance Malaysia Bhd.", //Hardcoded
                OrderNumber = model.OrderNo,
                ReferenceNumber = model.OrderNo,
                TotalAmount = model.Amt,
                ReturnUrl = model.RetUrl,
                AId = model.AId,
                TransactionNumber = transactionNumber,
                Mode = model.Mode,
                PaymentRef = model.PaymentRef
            };

            var fpxUtilities = new FPXUtilities();

            var individualBankList = fpxUtilities.GetBankList(FPXUtilities.MsgToken.INDIVIDUAL, transactionNumber);
            foreach (var bank in individualBankList.OrderBy(x => x.Value))
                viewModel.AvailableIndividualBanks.Add(new SelectListItem { Value = bank.Key, Text = bank.Value });
            viewModel.AvailableIndividualBanks.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

            //var corporateBankList = fpxUtilities.GetBankList(FPXUtilities.MsgToken.CORPORATE);
            //foreach (var bank in corporateBankList.OrderBy(x => x.Value))
            //    viewModel.AvailableCorporateBanks.Add(new SelectListItem { Value = bank.Key, Text = bank.Value });
            //viewModel.AvailableCorporateBanks.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

            //viewModel.AvailableTransactionTypes.Add(new SelectListItem { Value = "02", Text = "Company" });
            viewModel.AvailableTransactionTypes.Add(new SelectListItem { Value = "01", Text = "Individual" });
            viewModel.AvailableTransactionTypes.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

            return View("~/Views/FPXPayment/Summary.cshtml", viewModel);
        }


        [HttpPost, Route("Payment/InquiryFPX")]
        public ActionResult Inquiry(PaymentRequestVM model)
        {
            //for debugging purpose in production.
            LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(model));

            //check signature
            PaymentResultVM viewModelPaymentResultInquiry = new PaymentResultVM();
            var hashAuthentication = new HashAuthentication();
            if (!hashAuthentication.CompareRequestSignature(model.Amt.ToString(), model.OrderNo, model.AId,
                model.MsgSign))
            {
                var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", model.Amt), model.OrderNo,
                    int.Parse(PaymentResponseCode.InvalidSignature.ToString()), model.AId, string.Empty);

                viewModelPaymentResultInquiry = new PaymentResultVM()
                {
                    MsgSign = msgSign,
                    AId = model.AId,
                    Amt = string.Format("{0:.00}", model.Amt),
                    ReturnUrl = model.RetUrl,
                    Status = PaymentResponseCode.InvalidSignature.ToString(),
                    OrderNo = model.OrderNo,
                    IsEMandate = false,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef
                };
            }

           

            var svcPayment = new PaymentService();
            InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
            {
                OrderNo = model.OrderNo,
                PaymentProviderType = (int)PaymentProviderType.FPX,
                AId = model.AId,
                MsgSign = model.MsgSign,
                Amt = model.Amt.ToString()
            };
            var inquiryResult = svcPayment.InquiryPayment(inquiryPaymentInputModel);
            if (inquiryResult != null)
            {
                var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", inquiryResult.Amt), inquiryResult.OrderNo,
                    int.Parse(inquiryResult.Status), inquiryResult.ApplicationAccountId.ToString(), string.Empty);

                viewModelPaymentResultInquiry = new PaymentResultVM()
                {
                    MsgSign = msgSign,
                    AId = model.AId,
                    Amt = string.Format("{0:.00}", inquiryResult.Amt),
                    ReturnUrl = model.RetUrl,
                    Status = inquiryResult.Status,
                    OrderNo = model.OrderNo,
                    Bank = inquiryResult.Bank != null ? inquiryResult.Bank : string.Empty,
                    RefNo = inquiryResult.RefNo,
                    AuthCode = inquiryResult.AuthCode,
                    AuthNo = inquiryResult.AuthNo,
                    SellerId = inquiryResult.SellerId,
                    CreatedOn = inquiryResult.CreatedOn,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef,
                    IsEMandate = false,
                    LABankKey = inquiryResult.LABankKey,
                    LABankName = inquiryResult.LABankName
                };
            }

            return Json(viewModelPaymentResultInquiry);
        }

        [HttpPost, Route("Payment/Cancel")]
        public ActionResult CancelPayment(PaymentSummaryVM model)
        {
            var svcPayment = new PaymentService();
            var cancelResult = svcPayment.CancelPayment(model.TransactionNumber);

            var transactionStatus = PaymentResponseCode.UserCancel;
            var hashAuthentication = new HashAuthentication();
            var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", model.TotalAmount), model.OrderNumber,
                transactionStatus, model.AId, string.Empty);

            var viewModel = new PaymentResultVM()
            {
                MsgSign = msgSign,
                AId = model.AId,
                Amt = string.Format("{0:.00}", model.TotalAmount),
                ReturnUrl = model.ReturnUrl,
                Status = transactionStatus.ToString(),
                OrderNo = model.OrderNumber,
                CreatedOn = DateTime.Now
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                Mode = model.Mode,
                PaymentRef = model.PaymentRef,
                IsEMandate = false
            };

            return View("~/Views/Payment/Result.cshtml", viewModel);
        }

        [HttpPost, Route("Payment/Timeout")]
        public ActionResult TimeoutPayment(PaymentSummaryVM model)
        {
            var svcPayment = new PaymentService();
            var transactionStatus = PaymentResponseCode.Timeout;
            var timeoutResult = svcPayment.FailPaymentWithStatus(model.TransactionNumber, transactionStatus.ToString());

            var hashAuthentication = new HashAuthentication();
            var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", model.TotalAmount), model.OrderNumber,
                transactionStatus, model.AId, string.Empty);

            var viewModel = new PaymentResultVM()
            {
                MsgSign = msgSign,
                AId = model.AId,
                Amt = string.Format("{0:.00}", model.TotalAmount),
                ReturnUrl = model.ReturnUrl,
                Status = transactionStatus.ToString(),
                OrderNo = model.OrderNumber,
                CreatedOn = DateTime.Now
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                Mode = model.Mode,
                PaymentRef = model.PaymentRef,
                IsEMandate = false
            };

            return View("~/Views/Payment/Result.cshtml", viewModel);
        }

        [HttpPost, Route("Payment/Redirect")]
        public ActionResult RedirectPayment(PaymentSummaryVM model)
        {
            try
            {
                var svcPayment = new PaymentService();

                svcPayment.UpdatePaymentInformation(new UpdatePaymentInformationInputModel()
                {
                    TransactionNumber = model.TransactionNumber,
                    BuyerEmail = model.BuyerEmail,
                    MsgToken = model.TransactionType
                });

                var htmlResult = svcPayment.GetHtml(new GetHtmlInputModel()
                {
                    TransactionType = model.TransactionType,
                    BuyerEmail = model.BuyerEmail,
                    BuyerBank = model.BuyerBank,
                    TransactionNumber = model.TransactionNumber
                });
  
                if (htmlResult.Code == ResponseReturnCode.Gen_Success)
                {
                    return View("~/Views/Payment/PaymentGatewayRedirector.cshtml", new PaymentGatewayRedirectorVM()
                    {
                        FormHtml = htmlResult.FormHtml
                    });
                }

                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Failed to redirect. An error has occurred"
                });
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex.InnerException);
                //throw new HttpResponseException(HttpStatusCode.InternalServerError);
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_InternalServerError,
                    Description = ex.Message
                });
            }
        }

        /// <summary>
        /// Indirect callback
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Payment/fpx/Response_in")]
        public ActionResult IndirectResponsePayment()
        {
            try
            {
                var svcPayment = new PaymentService();
                var paymentResult = svcPayment.ProcessPayment(new ProcessPaymentInputModel()
                {
                    Form = DictHelper.BuildDictFromNVC(Request.Form),
                    PaymentProviderId = (int)PaymentProviderType.FPX
                });

                if (paymentResult.Code != ResponseReturnCode.Gen_Success)
                {
                    return View("~/Views/Error/Index.cshtml", new ErrorVM()
                    {
                        Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                        Description = "Invalid callback"
                    });
                }

                #region Build View Model

                var svcBankService = new BankService();
                var bank = svcBankService.GetBankList().BankList.FirstOrDefault(x => x.BankCode == paymentResult.Bank);
                

                var hashAuthentication = new HashAuthentication();
                var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", paymentResult.Amount), paymentResult.OrderNumber,
                    int.Parse(paymentResult.PaymentResponseCode), paymentResult.ApplicationAccountCode, string.Empty);

                var viewModel = new PaymentResultVM()
                {
                    MsgSign = msgSign,
                    AId = paymentResult.ApplicationAccountCode,
                    Amt = string.Format("{0:.00}", paymentResult.Amount),
                    ReturnUrl = paymentResult.ReturnUrl,
                    Status = paymentResult.PaymentResponseCode,
                    OrderNo = paymentResult.OrderNumber,
                    Bank = paymentResult.Bank != null ? paymentResult.Bank : string.Empty,
                    BankName = bank.BankName != null ? bank.BankName : string.Empty,
                    RefNo = paymentResult.ReferenceNumber,
                    AuthCode = paymentResult.AuthorizationCode,
                    AuthNo = paymentResult.AuthorizationNumber,
                    SellerId = paymentResult.MerchantId,
                    CreatedOn = paymentResult.CreatedOn
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                    PaymentRef = paymentResult.PaymentRef,
                    Mode = paymentResult.Mode,
                    IsEMandate = false,
                    LABankKey = paymentResult.LAFPXBankKey,
                    LABankName = paymentResult.LAFPXBankName
                };

                #endregion

                return View("~/Views/Payment/Result.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);
                //throw new HttpResponseException(HttpStatusCode.InternalServerError);
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_InternalServerError,
                    Description = ex.Message
                });
            }
        }

        /// <summary>
        /// Direct callback
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Payment/fpx/Response")]
        public ActionResult DirectResponsePayment()
        {
            try
            {
                var svcPayment = new PaymentService();
                var paymentResult = svcPayment.ProcessPayment(new BL.Data.PaymentProvider.ProcessPaymentInputModel()
                {
                    //Form = Request.Form,
                    PaymentProviderId = (int)PaymentProviderType.FPX
                });

                return Content("OK");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex.InnerException);
                return Content("NOT OK");
            }
        }
    }
}