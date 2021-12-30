using log4net;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.EMandate;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Service.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.PaymentProvider.FPX
{
    public class EMandateProcessor : IPaymentProcessor
    {
        bool disposed = false;

        public OutputModel InitiatePayment(InitiatePaymentInputModel model)
        {
            var result = new GetHtmlOutputModel();
            EmandateDecryptOutputModel EmandateOutputModel;

            try
            {
                ApplicationAccount applicationAccount;
                using (var repoApplicationAccount = new ApplicationAccountService())
                    applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(model.ApplicationAccountCode);

                EmandateDecryptInputModel EmandateInputModel = new EmandateDecryptInputModel
                {
                    EmandateSellerExchangeId = applicationAccount.EMandateSellerExchangeId,
                    EmandateSellerId = applicationAccount.EMandateSellerId
                };

                EmandateOutputModel = DecryptFPXHepler.EmandateDecrypt(EmandateInputModel);

                //insert into db first
                var currency = "MYR";
                using (var repoPayment = new EMandateTransactionRepository())
                    repoPayment.InsertEMandateTransaction(model.TransactionNumber, currency, model.Amount, model.OrderNumber,
                        applicationAccount.Id, model.ReturnUrl, EmandateOutputModel.EmandateSellerId, model.IdType, model.IdNo, 
                        model.ApplicationType, model.MaxFrequency, model.FrequencyMode, model.BuyerEmail, model.PayorName,
                        model.PurposeOfPayment, model.MobilePhoneNo, model.PaymentRef, model.MsgToken, model.Mode);

                result.Code = ResponseReturnCode.Gen_Success;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                result.Code = ResponseReturnCode.Gen_InternalServerError;
                result.Message = ex.ToString();
            }

            return result;
        }

        public GetHtmlOutputModel GenerateRequestHTML(GetHtmlInputModel model)
        {
            var result = new GetHtmlOutputModel();
            EmandateDecryptOutputModel EmandateOutputModel;

            try
            {
                EMandateTransaction paymentTransaction = null;
                using (var repoPayment = new EMandateTransactionRepository())
                    paymentTransaction = repoPayment.GetPaymentTransactionByTransactionNumber(model.TransactionNumber);

                ApplicationAccount applicationAccountInfo = null;
                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                }

                EmandateDecryptInputModel EmandateInputModel = new EmandateDecryptInputModel
                {
                    EmandateSellerExchangeId = applicationAccountInfo.EMandateSellerExchangeId,
                    EmandateSellerId = applicationAccountInfo.EMandateSellerId
                };

                EmandateOutputModel = DecryptFPXHepler.EmandateDecrypt(EmandateInputModel);

                //process html
                var buyerEmail = model.BuyerEmail == null ? "": model.BuyerEmail.Length > 27 ? "" : model.BuyerEmail;
                var totalAmount = string.Format("{0:.00}", paymentTransaction.Amount * Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION));
                var msgType = "AD";
                var msgToken = paymentTransaction.MsgToken == null ? "01": paymentTransaction.MsgToken;
                var txnTime = paymentTransaction.CreatedOn.ToString("yyyyMMyyHHmmss");
                var buyerId = paymentTransaction.IdNo + "," + paymentTransaction.IdType;
                var buyerIban = paymentTransaction.ApplicationType + "," +                      // application type
                                "" + "," +                                                      // phone number
                                paymentTransaction.MaxFrequency.ToString() + "," +              // maximum frequency
                                paymentTransaction.FrequencyMode + "," +                        // frequency mode
                                paymentTransaction.CreatedOn.ToString("ddMMyy") + "," +         // effective date
                                ""                                                              // expiry date
                                ;

                var fpxUtilities = new FPXUtilities();

                var orderNumber = fpxUtilities.GetOrdernum(paymentTransaction.OrderNumber, paymentTransaction.PaymentRef,
                    paymentTransaction.Mode);

                var checkSum = "||" + model.BuyerBank + "|" + buyerEmail + "|"+ buyerIban +"|"+ buyerId +"|||" + msgToken + "|" + msgType + "|" + paymentTransaction.Descriptions +
                    "|" + applicationAccountInfo.EMandateSellerBankCode + "|" + EmandateOutputModel.EmandateSellerExchangeId + "|" + paymentTransaction.TransactionNumber + "|" +
                    EmandateOutputModel.EmandateSellerId + "|" + orderNumber + "|" + txnTime + "|" + totalAmount + "|" + paymentTransaction.Currency + "|" +
                    applicationAccountInfo.EMandateFPX_Version;
                checkSum = checkSum.Trim();

                
                checkSum = fpxUtilities.RSASign(applicationAccountInfo.EMandatePrivateKeyPath, checkSum);

                var sb = new StringBuilder();

                sb.AppendLine(@"<form method=""post"" action=""" + EMandateSettings.BASE_URL + @"FPXMain/seller2DReceiver.jsp"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + msgType + @""" name=""fpx_msgType"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + msgToken + @""" name=""fpx_msgToken"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + EmandateOutputModel.EmandateSellerExchangeId + @""" name=""fpx_sellerExId"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.TransactionNumber + @""" name=""fpx_sellerExOrderNo"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + txnTime + @""" name=""fpx_sellerTxnTime"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + orderNumber + @""" name=""fpx_sellerOrderNo"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + EmandateOutputModel.EmandateSellerId + @""" name=""fpx_sellerId"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + applicationAccountInfo.EMandateSellerBankCode + @""" name=""fpx_sellerBankCode"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.Currency + @""" name=""fpx_txnCurrency"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + totalAmount + @""" name=""fpx_txnAmount"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + buyerEmail + @""" name=""fpx_buyerEmail"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + checkSum + @""" name=""fpx_checkSum"" />");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerName"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + model.BuyerBank + @""" name=""fpx_buyerBankId"" />");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerBankBranch"" />");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerAccNo"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + buyerId + @""" name=""fpx_buyerId"" />");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_makerName"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + buyerIban + @""" name=""fpx_buyerIban"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.Descriptions + @""" name=""fpx_productDesc"" />");
                sb.AppendLine(@"<input type=""hidden"" value=""" + applicationAccountInfo.EMandateFPX_Version + @""" name=""fpx_version"" />");
                sb.AppendLine(@"</form>");

                result.Code = ResponseReturnCode.Gen_Success;
                result.FormHtml = sb.ToString();
                LogManager.GetLogger(this.GetType()).Info("AD message : " + result.FormHtml);
                using (var emandateLogTransaction = new EMandateLogRepository())
                {
                    sb.Replace(checkSum, "<CheckSum>");
                    sb.Replace(EmandateOutputModel.EmandateSellerExchangeId, "<Seller_Exchange_ID>");
                    sb.Replace(EmandateOutputModel.EmandateSellerId, "<Seller_ID>");

                    emandateLogTransaction.InsertEMandateLog(paymentTransaction.OrderNumber,
                        EMandateSettings.BASE_URL + @"FPXMain/seller2DReceiver.jsp", sb.ToString(), "");
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                result.Code = ResponseReturnCode.Gen_InternalServerError;
                result.Message = ex.ToString();
            }

            return result;
        }

        public ProcessPaymentOutputModel ProcessPayment(ProcessPaymentInputModel model)
        {
            var result = new ProcessPaymentOutputModel()
            {
                PaymentProviderType = PaymentProviderType.EMandate
            };

            try
            {
                var buyerBankBranch = model.Form["fpx_buyerBankBranch"];
                var buyerBankId = model.Form["fpx_buyerBankId"];
                var buyerIban = model.Form["fpx_buyerIban"];
                var buyerId = model.Form["fpx_buyerId"];
                var buyerName = model.Form["fpx_buyerName"];
                var creditAuthCode = model.Form["fpx_creditAuthCode"];
                var creditAuthNo = model.Form["fpx_creditAuthNo"];
                var debitAuthCode = model.Form["fpx_debitAuthCode"];
                var debitAuthNo = model.Form["fpx_debitAuthNo"];
                var fpxTxnId = model.Form["fpx_fpxTxnId"];
                var fpxTxnTime = model.Form["fpx_fpxTxnTime"];
                var makerName = model.Form["fpx_makerName"];
                var msgToken = model.Form["fpx_msgToken"];
                var msgType = model.Form["fpx_msgType"];
                var sellerExId = model.Form["fpx_sellerExId"];
                var sellerExOrderNo = model.Form["fpx_sellerExOrderNo"];
                var sellerId = model.Form["fpx_sellerId"];
                var sellerOrderNo = model.Form["fpx_sellerOrderNo"];
                var sellerTxnTime = model.Form["fpx_sellerTxnTime"];
                var txnAmount = model.Form["fpx_txnAmount"];
                var txnCurrency = model.Form["fpx_txnCurrency"];
                var checkSum = model.Form["fpx_checkSum"];
                var checkSumString = buyerBankBranch + "|" + buyerBankId + "|" + buyerIban + "|" + buyerId + "|" + buyerName + "|" + creditAuthCode + "|" +
                    creditAuthNo + "|" + debitAuthCode + "|" + debitAuthNo + "|" + fpxTxnId + "|" + fpxTxnTime + "|" + makerName + "|" + msgToken + "|" + msgType + "|" +
                    sellerExId + "|" + sellerExOrderNo + "|" + sellerId + "|" + sellerOrderNo + "|" + sellerTxnTime + "|" + txnAmount + "|" + txnCurrency;

                EMandateTransaction paymentTransactionInfo = null;
                using (var repoPayment = new EMandateTransactionRepository())
                    paymentTransactionInfo = repoPayment.GetPaymentTransactionByTransactionNumber(sellerExOrderNo);

                ApplicationAccount applicationAccountInfo = null;
                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransactionInfo.ApplicationAccountId);
                }

                var fpxUtilities = new FPXUtilities();
                var finalVerifiMsg = fpxUtilities.nvl_VerifiMsg(applicationAccountInfo.EMandatePublicCertPath, checkSumString, checkSum);
                if (finalVerifiMsg == "00")
                    result.Code = ResponseReturnCode.Gen_Success;
                else
                    result.Code = ResponseReturnCode.Gen_Error_Occur;

                if (result.Code != ResponseReturnCode.Gen_Success)
                {
                    return result;
                }

                var svcBankService = new BankService();
                var bank = svcBankService.GetBankList().BankList.FirstOrDefault(x => x.BankCode == buyerBankId);
                if (bank != null)
                    result.Bank = bank.BankCode;
                else
                    result.Bank = "";

                var LAFpxBankKey = getFPXbank(buyerBankId);
                result.LAFPXBankKey = LAFpxBankKey.LABankKey;
                result.LAFPXBankName = LAFpxBankKey.BankName;

                result.TransactionNumber = sellerExOrderNo;
                //result.OrderNumber = sellerOrderNo;
                result.ReferenceNumber = fpxTxnId;
                result.AuthorizationCode = debitAuthCode;
                result.AuthorizationNumber = debitAuthNo;

                string[] buyerIbanValues = buyerIban.Split(',').Select(sValue => sValue.Trim()).ToArray();
                string maxFrequency = buyerIbanValues[2];
                string frequencyMode = buyerIbanValues[3];

                //result.DirectDebitAmount = Decimal.Parse(txnAmount);
                result.Amount = 1;
                result.MaxFrequency = Convert.ToInt32(maxFrequency);
                result.FrequencyMode = frequencyMode;
                result.Payload = JsonConvert.SerializeObject(model.Form);

                //update db
                var responseCode = PaymentResponseCode.Failed;
                using (var repoResponseCode = new ResponseCodeRepository())
                {
                    var dbResponseCode = repoResponseCode.GetResponseCodeByPaymentProviderCode(PaymentProviderType.FPX.ToString(), debitAuthCode);
                    if (dbResponseCode != null)
                    {
                        responseCode = dbResponseCode.TMLMStatus;
                        result.Message = dbResponseCode.Description;
                    }
                }
                result.PaymentResponseCode = responseCode.ToString();

                using (var repoPayment = new EMandateTransactionRepository())
                {
                    repoPayment.UpdatePaymentTransaction(sellerExOrderNo, fpxTxnId, buyerBankId == "" ? null: buyerBankId, result.Payload,
                        debitAuthCode, debitAuthNo, responseCode);

                    var paymentTransaction = repoPayment.GetPaymentTransactionByTransactionNumber(sellerExOrderNo);
                    result.OrderNumber = fpxUtilities.GetOrdernum(paymentTransaction.OrderNumber, paymentTransaction.PaymentRef,
                        sellerOrderNo, paymentTransaction.Mode);
                    //result.Amount = paymentTransaction.Amount;
                    result.MerchantId = paymentTransaction.MerchantId;
                    result.ReturnUrl = paymentTransaction.ReturnUrl;
                    result.Mode = paymentTransaction.Mode;
                    result.PaymentRef = paymentTransaction.PaymentRef;
                    result.ProductDesc = paymentTransaction.Descriptions;
                    result.ApplicationType = paymentTransaction.ApplicationType;
                    result.DirectDebitAmount = paymentTransaction.Amount * Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION);

                    //return fpx_fpxTxnTime instead of paymentTransaction.CreatedOn
                    if (DateTime.TryParseExact(fpxTxnTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fpxDateTime))
                        result.CreatedOn = fpxDateTime;
                    else
                        result.CreatedOn = paymentTransaction.CreatedOn;

                    using (var repoApplicationAccount = new ApplicationAccountService())
                    {
                        var applicationAccount = repoApplicationAccount
                            .GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                        result.ApplicationAccountCode = applicationAccount.Code;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                result.Code = ResponseReturnCode.Gen_InternalServerError;
                result.Message = ex.ToString();
            }

            return result;
        }

        public OutputModel CancelPayment(string transactionNumber)
        {
            var result = new OutputModel();

            try
            {
                using (var repoPayment = new EMandateTransactionRepository())
                {
                    repoPayment.UpdatePaymentTransaction(transactionNumber, null, null, null, null, null, PaymentResponseCode.UserCancel);
                }

                result.Code = ResponseReturnCode.Gen_Success;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                result.Code = ResponseReturnCode.Gen_InternalServerError;
                result.Message = ex.ToString();
            }

            return result;
        }

        public OutputModel FailPaymentWithStatus(string transactionNumber, string status)
        {
            var result = new OutputModel();

            try
            {
                using (var repoPayment = new PaymentTransactionRepository())
                {
                    repoPayment.UpdatePaymentTransaction(transactionNumber, null, null, null, null, null, int.Parse(status));
                }

                result.Code = ResponseReturnCode.Gen_Success;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                result.Code = ResponseReturnCode.Gen_InternalServerError;
                result.Message = ex.ToString();
            }

            return result;
        }

        public InquiryPaymentOutputModel Inquiry(InquiryPaymentInputModel model)
        {
            InquiryPaymentOutputModel outputModel = new InquiryPaymentOutputModel();
            EMandateTransaction paymentTransaction = null;
            ApplicationAccount applicationAccountInfo = null;
            EmandateDecryptOutputModel EmandateOutputModel;

            //check signature
            var hashAuthentication = new HashAuthentication();
            if (!hashAuthentication.CompareRequestSignature(model.Amt.ToString(), model.OrderNo, model.AId,
                model.MsgSign))
            {
                outputModel.Code = ResponseReturnCode.Gen_Error_Occur;
                outputModel.Message = "Invalid Signature";
                return outputModel;
            }

            using (var svcPaymentTrans = new EMandateTransactionService())
            {
                var paymentProvideType = (PaymentProviderType)model.PaymentProviderType;
                paymentTransaction = svcPaymentTrans.GetPaymentTransactionByOrderNumber(model.OrderNo, paymentProvideType.ToString());
            }

            using (var repoPayment = new ApplicationAccountService())
            {
                applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
            }

            EmandateDecryptInputModel EmandateInputModel = new EmandateDecryptInputModel
            {
                EmandateSellerExchangeId = applicationAccountInfo.EMandateSellerExchangeId,
                EmandateSellerId = applicationAccountInfo.EMandateSellerId
            };

            EmandateOutputModel = DecryptFPXHepler.EmandateDecrypt(EmandateInputModel);

            if (paymentTransaction == null)
            {
                outputModel.Code = ResponseReturnCode.Gen_RecordNotFound;
                outputModel.Message = "Invalid order number";
                return outputModel;
            }
            else
            {
                outputModel.Code = ResponseReturnCode.Gen_Success;
            }


            var svcBankService = new BankService();
            var bank = svcBankService.GetBankList().BankList.FirstOrDefault(x => x.BankCode == paymentTransaction.Bank);

            outputModel.Status = paymentTransaction.TransactionStatusId.ToString();
            outputModel.Mode = paymentTransaction.Mode;
            
            if (outputModel.Status == PaymentResponseCode.PendingPayment.ToString() || outputModel.Status == PaymentResponseCode.PendingAuthorization.ToString())
            {
                //call to fpx to get latest status
                try
                {
                    //get msgtoken thru bank
                    var msgToken = paymentTransaction.MsgToken == null ? "01" : paymentTransaction.MsgToken;
                    var msgType = "AE";
                    var txnTime = paymentTransaction.CreatedOn.ToString("yyyyMMyyHHmmss");
                    var buyerEmail = paymentTransaction.BuyerEmail == null ? "" : paymentTransaction.BuyerEmail.Length > 27 ? "" : paymentTransaction.BuyerEmail;
                    var totalAmount = string.Format("{0:.00}", 1);
                    var buyerId = paymentTransaction.IdNo + "," + paymentTransaction.IdType;
                    //var buyerIban = paymentTransaction.ApplicationType + "," +                      // application type
                    //                "" + "," +                                                      // phone number
                    //                paymentTransaction.MaxFrequency.ToString() + "," +              // maximum frequency
                    //                paymentTransaction.FrequencyMode + "," +                        // frequency mode
                    //                paymentTransaction.CreatedOn.ToString("ddMMyy") + "," +         // effective date
                    //                ""                                                              // expiry date
                    //                ;
                    var buyerIban = "";

                    var fpxUtilities = new FPXUtilities();
                    var orderNo = fpxUtilities.GetOrdernum(paymentTransaction.OrderNumber, paymentTransaction.PaymentRef,
                        paymentTransaction.Mode);

                    var unsigned = "||" + paymentTransaction.Bank + "|" + buyerEmail + "|" + buyerIban + "|" + buyerId + "|||" + msgToken + "|" + msgType + "|" + paymentTransaction.Descriptions +
                        "|" + applicationAccountInfo.EMandateSellerBankCode + "|" + EmandateOutputModel.EmandateSellerExchangeId + "|" + paymentTransaction.TransactionNumber + "|" +
                        EmandateOutputModel.EmandateSellerId + "|" + orderNo + "|" + txnTime + "|" + totalAmount + "|" + paymentTransaction.Currency + "|" +
                        applicationAccountInfo.EMandateFPX_Version;
                    unsigned = unsigned.Trim();
                   
                    var checksum = fpxUtilities.RSASign(applicationAccountInfo.EMandatePrivateKeyPath, unsigned);

                    string posting_data = "fpx_msgType=" + msgType + "&" + "fpx_msgToken=" + msgToken + "&" + "fpx_sellerExId="
                + EmandateOutputModel.EmandateSellerExchangeId + "&" + "fpx_sellerExOrderNo=" + paymentTransaction.TransactionNumber
                + "&" + "fpx_sellerTxnTime=" + txnTime + "&" + "fpx_sellerOrderNo=" + orderNo + "&" + "fpx_sellerBankCode=" + applicationAccountInfo.EMandateSellerBankCode
                + "&" + "fpx_txnCurrency=" + paymentTransaction.Currency + "&" + "fpx_txnAmount=" + totalAmount + "&" + "fpx_buyerEmail=" + buyerEmail + "&"
                + "fpx_checkSum=" + checksum + "&" + "fpx_buyerName=" + "&" + "fpx_buyerBankId=" + paymentTransaction.Bank
                + "&" + "fpx_buyerBankBranch=" + "&" + "fpx_buyerAccNo=" + "&" + "fpx_buyerId=" + buyerId + "&" + "fpx_makerName=" + "&" + "fpx_buyerIban=" + buyerIban
                + "&" + "fpx_productDesc=" + paymentTransaction.Descriptions + "&" + "fpx_version=" + applicationAccountInfo.EMandateFPX_Version + "&" + "fpx_sellerId="
                + EmandateOutputModel.EmandateSellerId + "&" + "checkSum_String=" + checksum;

                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                    string url = FPXSettings.BASE_URL + "FPXMain/sellerNVPTxnStatus.jsp";   //Here "URL" is my action method to receive HttpWebRequest
                    System.Net.HttpWebRequest objRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    objRequest.Method = "POST";
                    objRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                    objRequest.Accept = "application/json";
                    objRequest.AllowAutoRedirect = true;
                    objRequest.KeepAlive = false;
                    objRequest.Timeout = 300000;
                    byte[] _byteVersion = Encoding.ASCII.GetBytes(string.Concat(posting_data));
                    objRequest.ContentLength = _byteVersion.Length;
                    Stream stream = objRequest.GetRequestStream();
                    stream.Write(_byteVersion, 0, _byteVersion.Length);
                    stream.Close();
                    String resp = "";
                    String sHttpResonse = "";
                    using (System.Net.HttpWebResponse objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(objResponse.GetResponseStream()))
                        {
                            resp = reader.ReadToEnd();
                            Console.WriteLine(resp);
                        }
                        sHttpResonse = resp;
                    }
                    if (resp.Equals("ERROR") || resp.Contains("PROSESSING ERROR"))
                        return outputModel;

                    using (var fpxLogTransaction = new EMandateLogRepository())
                    {
                        fpxLogTransaction.InsertEMandateLog(paymentTransaction.OrderNumber, url, posting_data, resp);
                    }

                    var qscoll = System.Web.HttpUtility.ParseQueryString(resp);

                    String fpx_buyerBankBranch = qscoll["fpx_buyerBankBranch"];
                    String fpx_buyerBankId = qscoll["fpx_buyerBankId"];
                    String fpx_buyerIban = qscoll["fpx_buyerIban"];
                    String fpx_buyerId = qscoll["fpx_buyerId"];
                    String fpx_buyerName = qscoll["fpx_buyerName"];
                    String fpx_creditAuthCode = qscoll["fpx_creditAuthCode"];
                    String fpx_creditAuthNo = qscoll["fpx_creditAuthNo"];
                    String fpx_debitAuthCode = qscoll["fpx_debitAuthCode"];
                    String fpx_debitAuthNo = qscoll["fpx_debitAuthNo"];
                    String fpx_fpxTxnId = qscoll["fpx_fpxTxnId"];
                    String fpx_fpxTxnTime = qscoll["fpx_fpxTxnTime"];
                    String fpx_makerName = qscoll["fpx_makerName"];
                    String fpx_msgToken = qscoll["fpx_msgToken"];
                    String fpx_msgType = qscoll["fpx_msgType"];
                    String fpx_sellerExId = qscoll["fpx_sellerExId"];
                    String fpx_sellerExOrderNo = qscoll["fpx_sellerExOrderNo"];
                    String fpx_sellerId = qscoll["fpx_sellerId"];
                    String fpx_sellerOrderNo = qscoll["fpx_sellerOrderNo"];
                    String fpx_sellerTxnTime = qscoll["fpx_sellerTxnTime"];
                    String fpx_txnAmount = qscoll["fpx_txnAmount"];
                    String fpx_txnCurrency = qscoll["fpx_txnCurrency"];
                    String fpx_checkSum = qscoll["fpx_checkSum"];

                    var fpx_checkSumString = fpx_buyerBankBranch + "|" + fpx_buyerBankId + "|" + fpx_buyerIban + "|" + fpx_buyerId + "|" + fpx_buyerName + "|" + fpx_creditAuthCode + "|" + fpx_creditAuthNo + "|" + fpx_debitAuthCode + "|" + fpx_debitAuthNo + "|" + fpx_fpxTxnId + "|" + fpx_fpxTxnTime + "|" + fpx_makerName + "|" + fpx_msgToken + "|" + fpx_msgType + "|";
                    fpx_checkSumString += fpx_sellerExId + "|" + fpx_sellerExOrderNo + "|" + fpx_sellerId + "|" + fpx_sellerOrderNo + "|" + fpx_sellerTxnTime + "|" + fpx_txnAmount + "|" + fpx_txnCurrency;
                    var finalVerifiMsg = fpxUtilities.nvl_VerifiMsg(applicationAccountInfo.EMandatePublicCertPath, fpx_checkSumString, fpx_checkSum); 

                    if (finalVerifiMsg != "00")
                        return outputModel;

                    outputModel.Bank = string.IsNullOrEmpty(fpx_buyerBankId) ? paymentTransaction.Bank: fpx_buyerBankId;
                    outputModel.RefNo = fpx_fpxTxnId;
                    outputModel.AuthCode = fpx_debitAuthCode;
                    outputModel.AuthNo = fpx_debitAuthNo;
                    
                    //Get bank detail
                    var bankFPX = svcBankService.GetBankList().BankList.FirstOrDefault(x => x.BankCode == outputModel.Bank);
                    if (bankFPX != null)
                        outputModel.BankName = bankFPX.BankName;
                    else
                        outputModel.BankName = "";
                    //getLAFPXbankKey
                    var LAFpxBankKey = getFPXbank(outputModel.Bank);
                    outputModel.LABankKey = LAFpxBankKey.LABankKey;
                    outputModel.LABankName = LAFpxBankKey.BankName;

                    //outputModel.Amt = string.Format("{0:.00}", fpx_txnAmount);
                    outputModel.Amt = string.Format("{0:.00}", 1);
                    outputModel.OrderNo = paymentTransaction.OrderNumber;
                    outputModel.SellerId = paymentTransaction.MerchantId;
                    outputModel.CreatedOn = paymentTransaction.CreatedOn.ToString("dd MMM yyyy hh:mm:ss tt");
                    outputModel.ApplicationAccountId = paymentTransaction.ApplicationAccountId;
                    outputModel.PaymentRef = paymentTransaction.PaymentRef;
                    outputModel.MaxFrequency = paymentTransaction.MaxFrequency;
                    outputModel.FrequencyMode = paymentTransaction.FrequencyMode;
                    outputModel.ProductDesc = paymentTransaction.Descriptions;
                    outputModel.ApplicationType = paymentTransaction.ApplicationType;
                    outputModel.DirectDebitAmount = paymentTransaction.Amount * Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION);
                    outputModel.PaymentRef = paymentTransaction.PaymentRef;

                    //update with latest status
                    var responseCode = PaymentResponseCode.Failed;
                    using (var repoResponseCode = new ResponseCodeRepository())
                    {
                        var dbResponseCode = repoResponseCode.GetResponseCodeByPaymentProviderCode(PaymentProviderType.FPX.ToString(), fpx_debitAuthCode);
                        if (dbResponseCode != null)
                        {
                            responseCode = dbResponseCode.TMLMStatus;
                            outputModel.Message = dbResponseCode.Description;
                        }
                    }
                    outputModel.Status = responseCode.ToString();

                    // does not make sense to update if not found
                    // should not be not found if record exist on our side
                    // to be looked at individually if any
                    if (fpx_debitAuthCode != "76")  
                    {
                        using (var repoPayment = new EMandateTransactionRepository())
                        {
                            repoPayment.UpdatePaymentTransaction(fpx_sellerExOrderNo, fpx_fpxTxnId, null, JsonConvert.SerializeObject(qscoll),
                                fpx_debitAuthCode, fpx_debitAuthNo, responseCode);
                        }
                    }
                    

                    var applicationAccountCode = string.Empty;
                    using (var repoApplicationAccount = new ApplicationAccountService())
                    {
                        var applicationAccount = repoApplicationAccount
                            .GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                        applicationAccountCode = applicationAccount.Code;
                    }

                    outputModel.MsgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", 1), paymentTransaction.OrderNumber,
                        paymentTransaction.TransactionStatusId, applicationAccountCode, string.Empty);

                    outputModel.Code = ResponseReturnCode.Gen_Success;
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

                    outputModel.Code = ResponseReturnCode.Gen_InternalServerError;
                    outputModel.Message = ex.ToString();
                }
            }
            else if (outputModel.Status == PaymentResponseCode.Success.ToString() || !string.IsNullOrEmpty(paymentTransaction.AuthorizationCode))
            {
                outputModel.Bank = paymentTransaction.Bank;

                if (bank != null)
                    outputModel.BankName = bank.BankName;
                else
                    outputModel.BankName = "";
                outputModel.Amt = string.Format("{0:.00}", 1);
                outputModel.OrderNo = paymentTransaction.OrderNumber;
                outputModel.RefNo = paymentTransaction.PaymentReferenceNumber;
                outputModel.AuthCode = paymentTransaction.AuthorizationCode;
                outputModel.AuthNo = paymentTransaction.AuthorizationNumber;
                outputModel.SellerId = paymentTransaction.MerchantId;
                outputModel.CreatedOn = paymentTransaction.CreatedOn.ToString("dd MMM yyyy hh:mm:ss tt");
                outputModel.ApplicationAccountId = paymentTransaction.ApplicationAccountId;
                outputModel.ApplicationType = paymentTransaction.ApplicationType;
                outputModel.MaxFrequency = paymentTransaction.MaxFrequency;
                outputModel.FrequencyMode = paymentTransaction.FrequencyMode;
                outputModel.ProductDesc = paymentTransaction.Descriptions;
                outputModel.PaymentRef = paymentTransaction.PaymentRef;
                outputModel.DirectDebitAmount = paymentTransaction.Amount * Convert.ToInt32(EMandateSettings.AMT_MULTIPLICATION);

                var LAFpxBankKey = getFPXbank(outputModel.Bank);
                outputModel.LABankKey = LAFpxBankKey.LABankKey;
                outputModel.LABankName = LAFpxBankKey.BankName;
            }

            return outputModel;
        }

        private FpxBank getFPXbank(string FPXBankID)
        {
            var LAfpxBankService = new LAFpxBankService();
            var LAFPXbankKey = LAfpxBankService.GetLAFpxBank(FPXBankID);
            if (LAFPXbankKey == null)
            {
                LAFPXbankKey = LAfpxBankService.GetLAFpxBank("Other");
            }
            return LAFPXbankKey;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            // Free any unmanaged objects here.
            if (disposing)
            {
                //stuff to dispose
            }
            disposed = true;
        }
    }
}
