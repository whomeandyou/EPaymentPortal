using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.WebApi.ViewModels;
using TMLM.EPayment.BL.Data.RazerPay;
using System.Net.Http;
using System.Threading.Tasks;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.PaymentProvider.RazerPay;
using TMLM.EPayment.Db.Tables;
using TMLM.Common;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.WebApi.ViewModels.RazerPay;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class RazerPayController : Controller
    {
        // GET: RazerPay
        [HttpPost, Route("RazerPay/Request")]
        public ActionResult RequestView(PaymentRequestInputModel model)
        {
            try
            {
                //for debugging purpose in production.
                LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(model));

                //check signature
                var hashAuthentication = new HashAuthentication();
                if (!hashAuthentication.CompareRequestSignature(model.Amount, model.OrderNo, model.AId, model.MsgSign))
                {
                    return View("~/Views/Error/Index.cshtml", new ErrorVM()
                    {
                        Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                        Description = "Invalid Signature"
                    });
                }

                var svcPayment = new PaymentService();
                var transactionNumber = Guid.NewGuid().ToString();


                var initiateResult = svcPayment.InitiatePayment(new InitiatePaymentInputModel()
                {
                    ApplicationAccountCode = model.AId,
                    Amount = Convert.ToDecimal(model.Amount),
                    PaymentProviderId = (int)PaymentProviderType.RazerPay,
                    TransactionNumber = transactionNumber,
                    OrderNumber = model.OrderNo,
                    CreatedOn = DateTime.Now,
                    ReturnUrl = model.ReturnUrl,
                    IdType = string.Empty,
                    IdNo = string.Empty,
                    ApplicationType = string.Empty,
                    MaxFrequency = 0,
                    FrequencyMode = string.Empty,
                    BuyerEmail = model.BuyerEmail,
                    BuyerName = model.BuyerName,
                    PayorName = string.Empty,
                    PurposeOfPayment = string.Empty,
                    MobilePhoneNo = model.BuyerContact,
                    Mode = 0,
                    PaymentRef = string.Empty,
                    MsgToken = string.Empty,
                    Description = model.Description,
                    Currency = "MYR",
                    CancelUrl = model.CancelUrl
                });

                var htmlResult = svcPayment.GetHtml(new GetHtmlInputModel
                {
                    TransactionType = string.Empty,
                    TransactionNumber = transactionNumber,
                    BuyerBank = string.Empty,
                    BuyerEmail = string.Empty
                });

                if (htmlResult.Code == Common.ResponseReturnCode.Gen_Success)
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

                //return Json(initiateResult);
            }
            catch (Exception e)
            {
                var error = new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Failed to redirect. An error has occurred"
                };
                return Json(error);
            }
        }

        [HttpPost, Route("RazerPay/RequestHTML")]
        public ActionResult RequestHTML(RequestHTMLModel model)
        {
            try
            {
                var svcPayment = new PaymentService();

                var htmlResult = svcPayment.GetHtml(new GetHtmlInputModel
                {
                    TransactionType = string.Empty,
                    TransactionNumber = model.TransactionNumber,
                    BuyerBank = string.Empty,
                    BuyerEmail = string.Empty
                });

                if (htmlResult.Code == Common.ResponseReturnCode.Gen_Success)
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
            catch (Exception e)
            {
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Failed to redirect. An error has occurred"
                });
            }
        }

        [HttpPost, Route("RazerPay/Response")]
        public ActionResult Response()
        {
            var htmlResult = string.Empty;
            var svcPayment = new PaymentService();
            PaymentTransaction paymentTransactionInfo = null;
            LARazerPayEWallet razerPayInfo = new LARazerPayEWallet();

            var paymentResult = svcPayment.ProcessPayment(new ProcessPaymentInputModel()
            {
                Form = DictHelper.BuildDictFromNVC(Request.Form),
                PaymentProviderId = (int)PaymentProviderType.RazerPay
            });

            using (var repoPayment = new PaymentTransactionRepository())
                paymentTransactionInfo = repoPayment.GetPaymentTransactionByOrderNumber(paymentResult.OrderNumber, PaymentProviderType.RazerPay.ToString());

            using (var repoLARazerPay = new LARazerPayEWalletRepository())
                razerPayInfo = repoLARazerPay.GetLARazerPayEWalletById(paymentTransactionInfo.Channel);

            htmlResult = $@"
<form action='{paymentTransactionInfo.ReturnUrl}?orderId={paymentResult.OrderNumber}' method='post'>
    <input type='hidden' name='orderNo' value='{paymentResult.OrderNumber}' />
    <input type='hidden' name='amount' value='{paymentTransactionInfo.Amount}' />
    <input type='hidden' name='paymentReferenceNumber' value='{paymentTransactionInfo.PaymentReferenceNumber}' />
    <input type='hidden' name='status' value='{paymentTransactionInfo.TransactionStatusId}' />
    <input type='hidden' name='channel' value='{razerPayInfo.LAEWalletKey}' />
    <input type='hidden' name='errorCode' value='{paymentTransactionInfo.ResponseCode}' />
    <input type='hidden' name='errorDescription' value='{paymentTransactionInfo.ErrorMessage}' />
    <input type='hidden' name='msgSign' value='' />
</form>
";
            if (paymentResult.Code == Common.ResponseReturnCode.Gen_Success)
            {
                return View("~/Views/Payment/PaymentGatewayRedirector.cshtml", new PaymentGatewayRedirectorVM()
                {
                    FormHtml = htmlResult
                });
            }

            return View("~/Views/Error/Index.cshtml", new ErrorVM()
            {
                Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                Description = "Failed to redirect. An error has occurred"
            });
        }

        [HttpPost, Route("RazerPay/RequestInquiry")]
        public async Task<ActionResult> RequestInquiry(InquiryRequestInputModel model)
        {

            LogManager.GetLogger(this.GetType()).Info("RequestInquiry : " + JsonConvert.SerializeObject(model));
            ApplicationAccount applicationAccount = new ApplicationAccount();
            RazerPayUtilities razerPayUtilities = new RazerPayUtilities();
            PaymentTransaction paymentTransactionInfo = new PaymentTransaction();

            var hashAuthentication = new HashAuthentication();
            if (!hashAuthentication.CompareRequestSignature(model.Amount, model.OrderNo, model.ApplicationAccountCode, model.MsgSign))
            {
                var paymentVMInvalidSign = new RazerPayInquiryResponseVM(model.OrderNo, "" , "Invalid Signature", PaymentResponseCode.InvalidSignature,"","");

                return Json(paymentVMInvalidSign);
             
            }

            using (var repoApplicationAccount = new ApplicationAccountRepository())
                applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(model.ApplicationAccountCode);

            var razerPayOutputModel = RazerPayDecrypt(applicationAccount.RazerPayPublicKey, applicationAccount.RazerPayPrivateKey, applicationAccount.RazerPayMerchantId);

            using (var repoPayment = new PaymentTransactionRepository())
                paymentTransactionInfo = repoPayment.GetPaymentTransactionByOrderNumber(model.OrderNo, PaymentProviderType.RazerPay.ToString());


            if (paymentTransactionInfo == null)
            {
                var paymentVMNotFound = new RazerPayInquiryResponseVM(model.OrderNo, "", "Transaction Not Found", PaymentResponseCode.TransactionNotFound,"","");

                return Json(paymentVMNotFound);
            }
            else if (paymentTransactionInfo.TransactionStatusId == PaymentResponseCode.PendingPayment)
            {
               
                var hash = razerPayUtilities.HashDirectStatusRequery(paymentTransactionInfo.OrderNumber, razerPayOutputModel.MerchantId, razerPayOutputModel.PublicKey, paymentTransactionInfo.Amount.ToString());
                RazerPayInquiryResponse razerPayInquiryResponse = await razerPayUtilities.CallIndirectStatusRequery(hash, razerPayOutputModel.MerchantId, paymentTransactionInfo.OrderNumber, paymentTransactionInfo.Amount.ToString());

                var responseIntegrity = razerPayUtilities.VerifyRazerInquiry(razerPayInquiryResponse.Amount, razerPayOutputModel.PublicKey, razerPayInquiryResponse.Domain,
                    razerPayInquiryResponse.TranID, razerPayInquiryResponse.StatCode, razerPayInquiryResponse.VrfKey);
               

                if (razerPayInquiryResponse.VrfKey == "-")
                {
                    using (var repoPayment = new PaymentTransactionRepository())
                        await repoPayment.SpUpdateStatus(model.OrderNo, PaymentResponseCode.TransactionNotFound, razerPayInquiryResponse.ErrorCode, razerPayInquiryResponse.ErrorDesc, razerPayInquiryResponse.StatCode,razerPayInquiryResponse.Channel,razerPayInquiryResponse.TranID);
                }
                else
                {
                    if (decimal.Parse(razerPayInquiryResponse.Amount) == paymentTransactionInfo.Amount)
                    {
                        var responseCode = PaymentResponseCode.Failed;
                        if (razerPayInquiryResponse.StatCode == "00")
                            responseCode = PaymentResponseCode.Success;

                        using (var repoPayment = new PaymentTransactionRepository())
                            await repoPayment.SpUpdateStatus(model.OrderNo, responseCode, razerPayInquiryResponse.ErrorCode, razerPayInquiryResponse.ErrorDesc, razerPayInquiryResponse.StatCode, razerPayInquiryResponse.Channel,razerPayInquiryResponse.TranID);

                    }
                }

            }
            LARazerPayEWallet lARazerPayEWallet = new LARazerPayEWallet();
          
            using (var repoPayment = new PaymentTransactionRepository())
                paymentTransactionInfo = repoPayment.GetPaymentTransactionByOrderNumber(model.OrderNo, PaymentProviderType.RazerPay.ToString());
            using (var repoLARazerPay = new LARazerPayEWalletRepository())
                lARazerPayEWallet = repoLARazerPay.GetLARazerPayEWalletById(paymentTransactionInfo.Channel);
            var paymentVM = new RazerPayInquiryResponseVM(paymentTransactionInfo.OrderNumber, paymentTransactionInfo.ResponseCode, paymentTransactionInfo.ErrorMessage, paymentTransactionInfo.TransactionStatusId,
                lARazerPayEWallet.LAEWalletKey,paymentTransactionInfo.PaymentReferenceNumber); 

            return Json(paymentVM);

        }

        [HttpPost, Route("RazerPay/ResponseInquiry")]
        public async Task ResponseInquiry()
        {
            LogManager.GetLogger(this.GetType()).Info("ResponseInquiry : " + JsonConvert.SerializeObject(Request.Form));

            RazerPayUtilities razerPayUtilities = new RazerPayUtilities();
            PaymentTransaction paymentTransactionInfo = new PaymentTransaction();

            var responseAmount = Request.Form["Amount"].ToString();
            var responseTranId = Request.Form["TranID"].ToString();
            var responseDomain = Request.Form["Domain"].ToString();
            var responseChannel = Request.Form["Channel"].ToString();
            var responseVrfKey = Request.Form["VrfKey"].ToString();
            var responseStatCode = Request.Form["StatCode"].ToString();
            var responseStatName = Request.Form["StatName"].ToString();
            var responseCurrency = Request.Form["Currency"].ToString();
            var responseOrderId = Request.Form["OrderID"].ToString();
            var responseErrorCode = Request.Form["ErrorCode"].ToString();
            var responseErrorDesc = Request.Form["ErrorDesc"].ToString();
            //var responseIntegrity = razerPayUtilities.VerifyRazerInquiry(responseAmount, RazerPaySettings.SecretKey, responseDomain, responseTranId, responseStatCode, responseVrfKey);
            using (var repoPayment = new PaymentTransactionRepository())
                paymentTransactionInfo = repoPayment.GetPaymentTransactionByOrderNumber(responseOrderId, PaymentProviderType.RazerPay.ToString());

            if (responseVrfKey == "-")
            {
              //  using (var repoPayment = new PaymentTransactionRepository())
                 //   await repoPayment.SpUpdateStatus(responseOrderId, PaymentResponseCode.TransactionNotFound, responseErrorCode, responseErrorDesc, responseStatCode, responseChannel);
            }
            else
            {
                var response = ToDictionary(Request.Form);

                using (var repo = new RazerPayLogRepository())
                {
                    string temp = new JavaScriptSerializer().Serialize(response);
                    temp.Replace(responseVrfKey, "<VrfKey>");

                    repo.InsertRazerPayLog(responseOrderId, "RazerPay/ResponseInquiry", "", temp);
                }


                //if (responseIntegrity && decimal.Parse(responseAmount) == paymentTransactionInfo.Amount)
                //{
                //    var status = razerPayUtilities.GetPaymentStatus(responseStatCode);
                //    using (var repoPayment = new PaymentTransactionRepository())
                //        await repoPayment.SpUpdateStatus(responseOrderId, status, responseErrorCode, responseErrorDesc, responseStatCode, responseChannel);


                //}
            }
        }

        private static RazerPayDecryptOutputModel RazerPayDecrypt(string publicKey, string secretKey, string merchantId)
        {
            RazerPayDecryptOutputModel outputModel = new RazerPayDecryptOutputModel();
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(publicKey))
                    outputModel.PublicKey = oRSA.Decrypt(privateKey, publicKey);
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(secretKey))
                    outputModel.PrivateKey = oRSA.Decrypt(privateKey, secretKey);
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(merchantId))
                    outputModel.MerchantId = oRSA.Decrypt(privateKey, merchantId);
            }

            return outputModel;
        }

        private static IDictionary<string, string> ToDictionary(NameValueCollection col)
        {
            var dict = new Dictionary<string, string>();

            foreach (string key in col.Keys)
            {
                dict.Add(key, col[key]);
            }

            return dict;
        }
    }
}