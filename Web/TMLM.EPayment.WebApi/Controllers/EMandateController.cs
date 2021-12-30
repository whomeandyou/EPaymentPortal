using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMLM.Common;
using TMLM.EPayment.BL.Data.EMandate;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider.FPX;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.WebApi.ViewModels;
using TMLM.EPayment.WebApi.ViewModels.FPX;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class EMandateController : Controller
    {
        [HttpPost, Route("EMandate/Request")]
        public ActionResult RequestView(EMandateRequestVM model)
        {
            LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(model));

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
            var transactionNumber = Guid.NewGuid().ToString();

            InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
            {
                OrderNo = model.OrderNo,
                PaymentProviderType = (int)PaymentProviderType.EMandate,
                AId = model.AId,
                MsgSign = model.MsgSign,
                Amt = model.Amt
            };
            var inquiryResult = svcPayment.InquiryPayment(inquiryPaymentInputModel);
            if (inquiryResult.Status == PaymentResponseCode.Success.ToString())
            {
                var viewModel = new PaymentResultVM()
                {
                    MsgSign = inquiryResult.MsgSign,
                    AId = model.AId,
                    Amt = string.Format("{0:.00}", inquiryResult.Amt),
                    ReturnUrl = model.RetUrl,
                    Status = inquiryResult.Status,
                    OrderNo = inquiryResult.OrderNo,
                    Bank = inquiryResult.Bank,
                    BankName = inquiryResult.BankName,
                    RefNo = inquiryResult.RefNo,
                    AuthCode = inquiryResult.AuthCode,
                    AuthNo = inquiryResult.AuthNo,
                    SellerId = inquiryResult.SellerId,
                    CreatedOn = inquiryResult.CreatedOn,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef,
                    MaxFrequency = inquiryResult.MaxFrequency.ToString(),
                    FrequencyMode = EMandateDefaults.FREQUENCY_MODE[inquiryResult.FrequencyMode],
                    ProductDesc = inquiryResult.ProductDesc,
                    ApplicationType = EMandateDefaults.APPLICATION_TYPE[inquiryResult.ApplicationType],
                    DirectDebitAmount = string.Format("{0:.00}", inquiryResult.DirectDebitAmount),
                    IsEMandate = true,
                    LABankKey = inquiryResult.LABankKey,
                    LABankName = inquiryResult.LABankName
                };

                return View("~/Views/EMandate/EMandateResult.cshtml", viewModel);
            } 
            else
            {
                var initiateResult = svcPayment.InitiatePayment(new InitiatePaymentInputModel()
                {
                    ApplicationAccountCode = model.AId,
                    Amount = Convert.ToDecimal(model.Amt),
                    PaymentProviderId = (int)PaymentProviderType.EMandate,
                    TransactionNumber = transactionNumber,
                    OrderNumber = model.OrderNo,
                    CreatedOn = DateTime.Now,
                    ReturnUrl = model.RetUrl,
                    IdType = model.IdType,
                    IdNo = model.IDNo,
                    ApplicationType = model.ApplicationType,
                    MaxFrequency = model.MaxFrequency,
                    FrequencyMode = model.FreqMode,
                    BuyerEmail = model.BuyerEmail,
                    PayorName = model.Name,
                    PurposeOfPayment = model.PurposeOfPayment,
                    MobilePhoneNo = model.MobilePhoneNo,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef,
                    MsgToken = model.MsgToken
                });

                if (initiateResult.Code != ResponseReturnCode.Gen_Success)
                {
                    return View("~/Views/Error/Index.cshtml", new ErrorVM()
                    {
                        Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                        Description = "Failed to initiate payment. " + initiateResult.Message
                    });
                }

                var viewModel = new EMandateSummaryVM()
                {
                    MerchantName = "Tokio Marine Life Insurance Malaysia Bhd.", //Hardcoded
                    OrderNumber = model.OrderNo,
                    ReferenceNumber = model.OrderNo,
                    TotalAmount = string.Format("{0:F2}", model.Amt),
                    MultiplyAmt = Convert.ToDecimal(model.Amt) * Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION),
                    ReturnUrl = model.RetUrl,
                    BuyerEmail = model.BuyerEmail,
                    AId = model.AId,
                    TransactionNumber = transactionNumber,
                    ApplicationType = model.ApplicationType,
                    Name = model.Name,
                    IDNo = model.IDNo,
                    MobilePhoneNo = model.MobilePhoneNo,
                    PurposeOfPayment = model.PurposeOfPayment,
                    Mode = model.Mode,
                    PaymentRef = model.PaymentRef,
                    MaxFrequency = model.MaxFrequency,
                    FrequencyMode = EMandateDefaults.FREQUENCY_MODE[model.FreqMode],
                    AmountMultiplySetting = Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION),
                    MsgToken = EMandateDefaults.MSG_TOKEN[model.MsgToken]
                };

                var fpxUtilities = new FPXUtilities();

                var individualBankList = fpxUtilities.GetBankList(FPXUtilities.MsgToken.INDIVIDUAL, transactionNumber, "emandate");
                foreach (var bank in individualBankList.OrderBy(x => x.Value))
                    viewModel.AvailableIndividualBanks.Add(new SelectListItem { Value = bank.Key, Text = bank.Value });
                viewModel.AvailableIndividualBanks.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

                var corporateBankList = fpxUtilities.GetBankList(FPXUtilities.MsgToken.CORPORATE, transactionNumber, "emandate");
                foreach (var bank in corporateBankList.OrderBy(x => x.Value))
                    viewModel.AvailableCorporateBanks.Add(new SelectListItem { Value = bank.Key, Text = bank.Value });
                viewModel.AvailableCorporateBanks.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

                viewModel.AvailableTransactionTypes.Add(new SelectListItem { Value = "02", Text = "Company" });
                viewModel.AvailableTransactionTypes.Add(new SelectListItem { Value = "01", Text = "Individual" });
                viewModel.AvailableTransactionTypes.Insert(0, new SelectListItem { Value = string.Empty, Text = "- Please Select -" });

                return View("~/Views/EMandate/EMandateSummary.cshtml", viewModel);
            }
                
        }

        [HttpPost, Route("EMandate/Redirect")]
        public ActionResult RedirectPayment(PaymentSummaryVM model)
        {
            try
            {
                var svcPayment = new PaymentService();

                var updateResult = svcPayment.UpdateEMandateInformation(new UpdatePaymentInformationInputModel()
                {
                    TransactionNumber = model.TransactionNumber,
                    BuyerBank = model.BuyerBank
                });

                if (updateResult.Code != ResponseReturnCode.Gen_Success)
                {
                    return View("~/Views/Error/Index.cshtml", new ErrorVM()
                    {
                        Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                        Description = "Failed to initiate payment. " + updateResult.Message
                    });
                }

                var htmlResult = svcPayment.GetEMandateHtml(new GetHtmlInputModel()
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

        [HttpPost, Route("EMandate/Cancel")]
        public ActionResult CancelPayment(PaymentSummaryVM model)
        {
            var svcPayment = new PaymentService();
            var cancelResult = svcPayment.CancelPayment(model.TransactionNumber);

            var transactionStatus = PaymentResponseCode.UserCancel;

            var updateResult = svcPayment.UpdateEMandateInformation(new UpdatePaymentInformationInputModel()
            {
                TransactionNumber = model.TransactionNumber,
                Status = transactionStatus
            });

            if (updateResult.Code != ResponseReturnCode.Gen_Success)
            {
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Failed to initiate payment. " + updateResult.Message
                });
            }

            var hashAuthentication = new HashAuthentication();
            var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", model.TotalAmount), model.OrderNumber,
                transactionStatus, model.AId, string.Empty);

            var viewModel = new PaymentResultVM()
            {
                MsgSign = msgSign,
                AId = model.AId,
                Amt = string.Format("{0:.00}", "1"),
                ReturnUrl = model.ReturnUrl,
                Status = transactionStatus.ToString(),
                OrderNo = model.OrderNumber,
                CreatedOn = DateTime.Now
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                Mode = model.Mode,
                PaymentRef = model.PaymentRef,
                //DirectDebitAmount = string.Format("{0:.00}", model.TotalAmount),
                IsEMandate = true
            };

            return View("~/Views/Payment/Result.cshtml", viewModel);
        }

        [HttpPost, Route("EMandate/Timeout")]
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
                Amt = string.Format("{0:.00}", "1"),
                ReturnUrl = model.ReturnUrl,
                Status = transactionStatus.ToString(),
                OrderNo = model.OrderNumber,
                CreatedOn = DateTime.Now
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                Mode = model.Mode,
                PaymentRef = model.PaymentRef,
                //DirectDebitAmount = string.Format("{0:.00}", model.TotalAmount),
                IsEMandate = true
            };

            return View("~/Views/Payment/Result.cshtml", viewModel);
        }

        [HttpPost, Route("EMandate/InquiryFPX")]
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
                    IsEMandate = true
                };
            }



            var svcPayment = new PaymentService();
            InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
            {
                OrderNo = model.OrderNo,
                PaymentProviderType = (int)PaymentProviderType.EMandate,
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
                    IsEMandate = true,
                    MaxFrequency = inquiryResult.MaxFrequency.ToString(),
                    FrequencyMode = inquiryResult.FrequencyMode,
                    ProductDesc = inquiryResult.ProductDesc,
                    ApplicationType = inquiryResult.ApplicationType,
                    DirectDebitAmount = string.Format("{0:.00}", inquiryResult.DirectDebitAmount),
                    LABankKey = inquiryResult.LABankKey,
                    LABankName = inquiryResult.LABankName
                };
            }

            return Json(viewModelPaymentResultInquiry);
        }

        /// <summary>
        /// Indirect callback
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("EMandate/success_in")]
        public ActionResult IndirectResponsePayment()
        {
            try
            {
                LogManager.GetLogger(this.GetType()).Info("Indirect callback response : " + Request.Form);
                var svcPayment = new PaymentService();
                var paymentResult = svcPayment.ProcessPayment(new ProcessPaymentInputModel()
                {
                    Form = DictHelper.BuildDictFromNVC(Request.Form),
                    PaymentProviderId = (int)PaymentProviderType.EMandate
                });

                if (paymentResult.Code != ResponseReturnCode.Gen_Success)
                {
                    return View("~/Views/Error/Index.cshtml", new ErrorVM()
                    {
                        Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                        Description = "Invalid callback"
                    });
                }

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
                    Bank = bank != null ? bank.BankCode : string.Empty,
                    BankName = bank != null ? bank.BankName : string.Empty,
                    RefNo = paymentResult.ReferenceNumber,
                    AuthCode = paymentResult.AuthorizationCode,
                    AuthNo = paymentResult.AuthorizationNumber,
                    SellerId = paymentResult.MerchantId,
                    CreatedOn = paymentResult.CreatedOn
                        .ToLocalTime()//need to manually convert to local as not sure why when api return it will consider as utc
                        .ToString("dd MMM yyyy hh:mm:ss tt"),
                    Mode = paymentResult.Mode,
                    PaymentRef = paymentResult.PaymentRef,
                    MaxFrequency = paymentResult.MaxFrequency.ToString(),
                    FrequencyMode = EMandateDefaults.FREQUENCY_MODE[paymentResult.FrequencyMode],
                    ProductDesc = paymentResult.ProductDesc,
                    ApplicationType = EMandateDefaults.APPLICATION_TYPE[paymentResult.ApplicationType],
                    DirectDebitAmount = string.Format("{0:.00}", paymentResult.DirectDebitAmount),
                    IsEMandate = true,
                    LABankKey = paymentResult.LAFPXBankKey,
                    LABankName = paymentResult.LAFPXBankName
                };

                return View("~/Views/EMandate/EMandateResult.cshtml", viewModel);
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
        [HttpPost, Route("EMandate/success_di")]
        public ActionResult DirectResponsePayment()
        {
            try
            {
                LogManager.GetLogger(this.GetType()).Info("Direct callback response : " + Request.Form);

                var svcPayment = new PaymentService();
                var paymentResult = svcPayment.ProcessPayment(new BL.Data.PaymentProvider.ProcessPaymentInputModel()
                {
                    //Form = Request.Form,
                    PaymentProviderId = (int)PaymentProviderType.EMandate
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