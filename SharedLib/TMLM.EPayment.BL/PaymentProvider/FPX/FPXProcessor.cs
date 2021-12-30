using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.FPXPayment;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Service.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.PaymentProvider.FPX
{
    public class FPXProcessor : IPaymentProcessor
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        public OutputModel InitiatePayment(InitiatePaymentInputModel model)
        {
            var result = new GetHtmlOutputModel();
            FpxDecryptOutputModel FpxOutputModel;
            try
            {
                ApplicationAccount applicationAccount;
                using (var repoApplicationAccount = new ApplicationAccountService())
                    applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(model.ApplicationAccountCode);

                FpxDecryptInputModel FpxInputModel = new FpxDecryptInputModel
                {
                    FPXSellerExchangeId = applicationAccount.FPXSellerExchangeId,
                    FPXSellerId = applicationAccount.FPXSellerId
                };

                FpxOutputModel = DecryptFPXHepler.FpxDecrypt(FpxInputModel);

                //insert into db first
                var currency = "MYR";
                using (var repoPayment = new PaymentTransactionRepository())
                    repoPayment.InsertPaymentTransaction(model.TransactionNumber, currency, model.Amount, model.OrderNumber,
                        applicationAccount.Id, model.ReturnUrl, FpxOutputModel.FPXSellerId, PaymentProviderType.FPX.ToString(), null, null, null, 
                        model.Mode, model.PaymentRef);

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
            FpxDecryptOutputModel FpxOutputModel;

            try
            {
                PaymentTransaction paymentTransaction = null;
                using (var repoPayment = new PaymentTransactionRepository())
                    paymentTransaction = repoPayment.GetPaymentTransactionByTransactionNumber(model.TransactionNumber);

                ApplicationAccount applicationAccountInfo = null;
                using(var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                }

                FpxDecryptInputModel FpxInputModel = new FpxDecryptInputModel
                {
                    FPXSellerExchangeId = applicationAccountInfo.FPXSellerExchangeId,
                    FPXSellerId = applicationAccountInfo.FPXSellerId
                };

                FpxOutputModel = DecryptFPXHepler.FpxDecrypt(FpxInputModel);

                //process html
                var totalAmount = string.Format("{0:.00}", paymentTransaction.Amount);
                var msgType = "AR";
                var productDesc = "Policy Payment";
                var txnTime = paymentTransaction.CreatedOn.ToString("yyyyMMyyHHmmss");

                var checkSum = "||" + model.BuyerBank + "|" + model.BuyerEmail + "|||||" + model.TransactionType + "|" + msgType + "|" + productDesc +
                    "|" + applicationAccountInfo.FPXSellerBankCode + "|" + FpxOutputModel.FPXSellerExchangeId + "|" + paymentTransaction.TransactionNumber + "|" +
                    FpxOutputModel.FPXSellerId + "|" + paymentTransaction.OrderNumber + "|" + txnTime + "|" + totalAmount + "|" + paymentTransaction.Currency + "|" +
                    applicationAccountInfo.FPXVersion;
                checkSum = checkSum.Trim();

                var fpxUtilities = new FPXUtilities();
                checkSum = fpxUtilities.RSASign(applicationAccountInfo.FPXPrivateKeyPath, checkSum);

                var sb = new StringBuilder();

                sb.AppendLine(@"<form method=""post"" action=""" + FPXSettings.BASE_URL + @"FPXMain/seller2DReceiver.jsp"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + msgType + @""" name=""fpx_msgType"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + model.TransactionType + @""" name=""fpx_msgToken"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + FpxOutputModel.FPXSellerExchangeId + @""" name=""fpx_sellerExId"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.TransactionNumber + @""" name=""fpx_sellerExOrderNo"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + txnTime + @""" name=""fpx_sellerTxnTime"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.OrderNumber + @""" name=""fpx_sellerOrderNo"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + applicationAccountInfo.FPXSellerBankCode + @""" name=""fpx_sellerBankCode"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + paymentTransaction.Currency + @""" name=""fpx_txnCurrency"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + totalAmount + @""" name=""fpx_txnAmount"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + model.BuyerEmail + @""" name=""fpx_buyerEmail"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + checkSum + @""" name=""fpx_checkSum"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerName"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + model.BuyerBank + @""" name=""fpx_buyerBankId"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerBankBranch"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerAccNo"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerId"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_makerName"">");
                sb.AppendLine(@"<input type=""hidden"" value="""" name=""fpx_buyerIban"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + productDesc + @""" name=""fpx_productDesc"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + applicationAccountInfo.FPXVersion + @""" name=""fpx_version"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + FpxOutputModel.FPXSellerId + @""" name=""fpx_sellerId"">");
                sb.AppendLine(@"<input type=""hidden"" value=""" + checkSum + @""" name=""checkSum_String"">");

                result.Code = ResponseReturnCode.Gen_Success;
                result.FormHtml = sb.ToString();
                using (var fpxLogTransaction = new FPXLogRepository())
                {
                    sb.Replace(checkSum, "<CheckSum>");
                    sb.Replace(FpxOutputModel.FPXSellerExchangeId, "<Seller_Exchange_ID>");
                    sb.Replace(FpxOutputModel.FPXSellerId, "<Seller_ID>");

                    fpxLogTransaction.InsertFPXLog(paymentTransaction.OrderNumber,
                        FPXSettings.BASE_URL + @"FPXMain/seller2DReceiver.jsp", sb.ToString(),"");
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
                PaymentProviderType = PaymentProviderType.FPX
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

                PaymentTransaction paymentTransactionInfo = null;
                using (var repoPayment = new PaymentTransactionRepository())
                    paymentTransactionInfo = repoPayment.GetPaymentTransactionByTransactionNumber(sellerExOrderNo);

                ApplicationAccount applicationAccountInfo = null;
                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransactionInfo.ApplicationAccountId);
                }

                var fpxUtilities = new FPXUtilities();
                var finalVerifiMsg = fpxUtilities.nvl_VerifiMsg(applicationAccountInfo.FPXPublicCertPath, checkSumString, checkSum);
                //return error if verification fails
                if (finalVerifiMsg == "00")
                    result.Code = ResponseReturnCode.Gen_Success;
                else
                    result.Code = ResponseReturnCode.Gen_Error_Occur;

                result.Bank = buyerBankId;
                result.TransactionNumber = sellerExOrderNo;
                result.OrderNumber = sellerOrderNo;
                result.ReferenceNumber = fpxTxnId;
                result.AuthorizationCode = debitAuthCode;
                result.AuthorizationNumber = debitAuthNo;
                //getLAFPXbank
                var LAFPXBank = getFPXbank(result.Bank);
                result.LAFPXBankKey = LAFPXBank.LABankKey;
                result.LAFPXBankName = LAFPXBank.BankName;


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

                using (var repoPayment = new PaymentTransactionRepository())
                {
                    repoPayment.UpdatePaymentTransaction(sellerExOrderNo, fpxTxnId, buyerBankId, result.Payload,
                        debitAuthCode, debitAuthNo, responseCode);

                    var paymentTransaction = repoPayment.GetPaymentTransactionByTransactionNumber(sellerExOrderNo);
                    result.Amount = paymentTransaction.Amount;
                    result.MerchantId = paymentTransaction.MerchantId;
                    result.ReturnUrl = paymentTransaction.ReturnUrl;
                    result.Mode = paymentTransaction.Mode;
                    result.PaymentRef = paymentTransaction.ProposalId;

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
                using (var repoPayment = new PaymentTransactionRepository())
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
            List<InquiryPaymentOutputModel> outputList = new List<InquiryPaymentOutputModel>();
            List<PaymentTransaction> paymentTransactionList = new List<PaymentTransaction>();
            ApplicationAccount applicationAccountInfo = null;
            FpxDecryptOutputModel FpxOutputModel;

            //check signature
            var hashAuthentication = new HashAuthentication();
            if (!hashAuthentication.CompareRequestSignature(model.Amt.ToString(), model.OrderNo, model.AId,
                model.MsgSign))
            {
                outputModel.Code = ResponseReturnCode.Gen_Error_Occur;
                outputModel.Message = "Invalid Signature";
                return outputModel;
            }

            using (var svcPaymentTrans = new PaymentTransactionService())
            {
                var paymentProvideType = (PaymentProviderType)model.PaymentProviderType;
                paymentTransactionList = svcPaymentTrans.GetPendingPaymentTransactionListByOrderNumber(model.OrderNo, paymentProvideType.ToString());
            }

            foreach(var paymentTransaction in paymentTransactionList)
            {
                outputModel = new InquiryPaymentOutputModel();
                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                }

                FpxDecryptInputModel FpxInputModel = new FpxDecryptInputModel
                {
                    FPXSellerExchangeId = applicationAccountInfo.FPXSellerExchangeId,
                    FPXSellerId = applicationAccountInfo.FPXSellerId
                };

                FpxOutputModel = DecryptFPXHepler.FpxDecrypt(FpxInputModel);

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

                if (outputModel.Status == PaymentResponseCode.PendingPayment.ToString() || outputModel.Status == PaymentResponseCode.PendingAuthorization.ToString()
                    || paymentTransaction.AuthorizationCode == "XO" || paymentTransaction.AuthorizationCode == "09")
                {
                    //call to fpx to get latest status
                    try
                    {
                        //get msgtoken thru bank
                        var msgToken = paymentTransaction.MsgToken;
                        var msgType = "AE";
                        var currency = "MYR";
                        var txnTime = paymentTransaction.CreatedOn.ToString("yyyyMMyyHHmmss");
                        var txnAmount = string.Format("{0:.00}", paymentTransaction.Amount);
                        var productDesc = "Policy Payment";

                        String fpx_checkSum = "";
                        fpx_checkSum = "||" + paymentTransaction.Bank + "|" + paymentTransaction.BuyerEmail + "||||";
                        fpx_checkSum += "|" + msgToken + "|" + msgType + "|" + productDesc + "|" + applicationAccountInfo.FPXSellerBankCode + "|"
                            + FpxOutputModel.FPXSellerExchangeId + "|";
                        fpx_checkSum += paymentTransaction.TransactionNumber + "|" + FpxOutputModel.FPXSellerId + "|" + paymentTransaction.OrderNumber + "|" + txnTime + "|" + txnAmount
                            + "|" + currency + "|" + applicationAccountInfo.FPXVersion;
                        fpx_checkSum = fpx_checkSum.Trim();
                        var fpxUtilities = new FPXUtilities();
                        var checksum = fpxUtilities.RSASign(applicationAccountInfo.FPXPrivateKeyPath, fpx_checkSum);

                        string posting_data = "fpx_msgType=" + msgType + "&" + "fpx_msgToken=" + msgToken + "&" + "fpx_sellerExId="
                    + FpxOutputModel.FPXSellerExchangeId + "&" + "fpx_sellerExOrderNo=" + paymentTransaction.TransactionNumber
                    + "&" + "fpx_sellerTxnTime=" + txnTime + "&" + "fpx_sellerOrderNo=" + paymentTransaction.OrderNumber + "&" + "fpx_sellerBankCode=" + applicationAccountInfo.FPXSellerBankCode
                    + "&" + "fpx_txnCurrency=" + currency + "&" + "fpx_txnAmount=" + txnAmount + "&" + "fpx_buyerEmail=" + paymentTransaction.BuyerEmail + "&"
                    + "fpx_checkSum=" + checksum + "&" + "fpx_buyerName=" + "&" + "fpx_buyerBankId=" + paymentTransaction.Bank
                    + "&" + "fpx_buyerBankBranch=" + "&" + "fpx_buyerAccNo=" + "&" + "fpx_buyerId=" + "&" + "fpx_makerName=" + "&" + "fpx_buyerIban="
                    + "&" + "fpx_productDesc=" + productDesc + "&" + "fpx_version=" + applicationAccountInfo.FPXVersion + "&" + "fpx_sellerId="
                    + FpxOutputModel.FPXSellerId + "&" + "checkSum_String=" + checksum;
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

                        using (var fpxLogTransaction = new FPXLogRepository())
                        {
                            fpxLogTransaction.InsertFPXLog(paymentTransaction.OrderNumber,
                                url, posting_data, resp);
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
                        fpx_checkSum = qscoll["fpx_checkSum"];

                        var fpx_checkSumString = fpx_buyerBankBranch + "|" + fpx_buyerBankId + "|" + fpx_buyerIban + "|" + fpx_buyerId + "|" + fpx_buyerName + "|" + fpx_creditAuthCode + "|" + fpx_creditAuthNo + "|" + fpx_debitAuthCode + "|" + fpx_debitAuthNo + "|" + fpx_fpxTxnId + "|" + fpx_fpxTxnTime + "|" + fpx_makerName + "|" + fpx_msgToken + "|" + fpx_msgType + "|";
                        fpx_checkSumString += fpx_sellerExId + "|" + fpx_sellerExOrderNo + "|" + fpx_sellerId + "|" + fpx_sellerOrderNo + "|" + fpx_sellerTxnTime + "|" + fpx_txnAmount + "|" + fpx_txnCurrency;
                        var finalVerifiMsg = fpxUtilities.nvl_VerifiMsg(applicationAccountInfo.FPXPublicCertPath, fpx_checkSumString, fpx_checkSum); //Certificate Path
                                                                                                                                                     //return error if verification fails
                        if (finalVerifiMsg != "00")
                            return outputModel;

                        outputModel.Bank = fpx_buyerBankId;
                        outputModel.RefNo = fpx_fpxTxnId;
                        outputModel.AuthCode = fpx_debitAuthCode;
                        outputModel.AuthNo = fpx_debitAuthNo;

                        //Get bank detail
                        var bankFPX = svcBankService.GetBankList().BankList.FirstOrDefault(x => x.BankCode == outputModel.Bank);
                        outputModel.BankName = bankFPX.BankName;
                        //Get Fpx Bank Key
                        var LAFpxBankKey = getFPXbank(outputModel.Bank);
                        outputModel.LABankKey = LAFpxBankKey.LABankKey;
                        outputModel.LABankName = LAFpxBankKey.BankName;


                        outputModel.Amt = string.Format("{0:.00}", fpx_txnAmount);
                        outputModel.OrderNo = paymentTransaction.OrderNumber;
                        outputModel.SellerId = paymentTransaction.MerchantId;
                        outputModel.CreatedOn = paymentTransaction.CreatedOn.ToString("dd MMM yyyy hh:mm:ss tt");
                        outputModel.ApplicationAccountId = paymentTransaction.ApplicationAccountId;

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

                        using (var repoPayment = new PaymentTransactionRepository())
                        {
                            repoPayment.UpdatePaymentTransaction(fpx_sellerExOrderNo, fpx_fpxTxnId, fpx_buyerBankId, JsonConvert.SerializeObject(qscoll),
                                fpx_debitAuthCode, fpx_debitAuthNo, responseCode);
                        }

                        var applicationAccountCode = string.Empty;
                        using (var repoApplicationAccount = new ApplicationAccountService())
                        {
                            var applicationAccount = repoApplicationAccount
                                .GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                            applicationAccountCode = applicationAccount.Code;
                        }

                        outputModel.MsgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", paymentTransaction.Amount), paymentTransaction.OrderNumber,
                            paymentTransaction.TransactionStatusId, applicationAccountCode, string.Empty);

                        outputModel.Code = ResponseReturnCode.Gen_Success;
                        outputList.Add(outputModel);
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
                    outputModel.BankName = bank.BankName;
                    outputModel.Amt = string.Format("{0:.00}", paymentTransaction.Amount);
                    outputModel.OrderNo = paymentTransaction.OrderNumber;
                    outputModel.RefNo = paymentTransaction.PaymentReferenceNumber;
                    outputModel.AuthCode = paymentTransaction.AuthorizationCode;
                    outputModel.AuthNo = paymentTransaction.AuthorizationNumber;
                    outputModel.SellerId = paymentTransaction.MerchantId;
                    outputModel.CreatedOn = paymentTransaction.CreatedOn.ToString("dd MMM yyyy hh:mm:ss tt");
                    outputModel.ApplicationAccountId = paymentTransaction.ApplicationAccountId;

                    var LAFpxBankKey = getFPXbank(outputModel.Bank);
                    outputModel.LABankKey = LAFpxBankKey.LABankKey;
                    outputModel.LABankName = LAFpxBankKey.BankName;

                    outputList.Add(outputModel);
                }
            }
            
            outputModel = outputList.Where(x => x.Status == PaymentResponseCode.Success.ToString()).OrderByDescending(x => x.CreatedOn).FirstOrDefault();

            if(outputModel == null)
            {
                outputModel = new InquiryPaymentOutputModel();
            }
            

            return outputModel;
        }

        private FpxBank getFPXbank(string FPXBankID)
        {
            var LAfpxBankService = new LAFpxBankService();
            var LAFPXbankKey = LAfpxBankService.GetLAFpxBank(FPXBankID);
            if(LAFPXbankKey == null)
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
