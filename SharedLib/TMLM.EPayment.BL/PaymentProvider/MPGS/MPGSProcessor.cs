using log4net;
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
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Service.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.PaymentProvider.MPGS
{
    public class MPGSProcessor : IPaymentProcessor
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        public OutputModel InitiatePayment(InitiatePaymentInputModel model)
        {
            var result = new OutputModel();
            try
            {
                ApplicationAccount applicationAccount;
                using (var repoApplicationAccount = new ApplicationAccountService())
                    applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(model.ApplicationAccountCode);

                //insert into db 
                using (var repoPaymentTransaction = new PaymentTransactionRepository())
                {
                    repoPaymentTransaction.InsertPaymentTransaction(model.TransactionNumber, model.Currency, model.Amount,
                        model.OrderNumber, applicationAccount.Id, model.ReturnUrl, MPGSSettings.MERCHANT_ID,
                        PaymentProviderType.MPGS.ToString(), isEnrolment: model.IsEnrolment,
                        isInitialPayment: model.IsInitialPayment, paymentRef: model.PaymentRef);
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

        public GetHtmlOutputModel GenerateRequestHTML(GetHtmlInputModel model)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentOutputModel ProcessPayment(ProcessPaymentInputModel model)
        {
            var result = new ProcessPaymentOutputModel()
            {
                PaymentProviderType = PaymentProviderType.MPGS
            };

            //below fields already massaged and handed from own application
            model.Form.TryGetValue("TransactionNumber", out var transactionNumber);
            model.Form.TryGetValue("ErrorMessage", out var errorMessage);
            model.Form.TryGetValue("CardNumber", out var cardNumber);
            model.Form.TryGetValue("ExpiryMonth", out var expiryMonth);
            model.Form.TryGetValue("ExpiryYear", out var expiryYear);
            model.Form.TryGetValue("CardType", out var cardType);
            model.Form.TryGetValue("CardMethod", out var cardMethod);
            model.Form.TryGetValue("TransCode", out var transCode);
            model.Form.TryGetValue("AppId", out var appId);
            model.Form.TryGetValue("BankName", out var bankName);
            model.Form.TryGetValue("ResponseCode", out var respCode);
            model.Form.TryGetValue("AuthCode", out var authCode);
            model.Form.TryGetValue("newTransactionId", out var newTransactionId);

            try
            {
                result.Code = ResponseReturnCode.Gen_Success;
                result.Bank = bankName;
                result.TransactionNumber = transactionNumber;
                result.AuthorizationCode = authCode;
                result.AuthorizationNumber = transCode;

                model.Form["CardNumber"] = "<creditCard>";
                result.Payload = JsonConvert.SerializeObject(model.Form);

                //update db
                var responseCode = PaymentResponseCode.Failed;
                using (var repoResponseCode = new ResponseCodeRepository())
                {
                    var dbResponseCode = repoResponseCode.GetResponseCodeByPaymentProviderCode(PaymentProviderType.MPGS.ToString(), respCode);
                    if (dbResponseCode != null)
                        responseCode = dbResponseCode.TMLMStatus;
                }
                result.PaymentResponseCode = responseCode.ToString();

                using (var repoPaymentTransaction = new PaymentTransactionRepository())
                {
                    string encryptedCardNumber = cardNumber;
                    if (encryptedCardNumber.Length < 20)
                    {
                        var ivKey = BitConverter.ToString(Encoding.ASCII.GetBytes(RandomHelper.RandomString(16))).Replace("-", string.Empty);
                        encryptedCardNumber = AESMethod.EncryptString(cardNumber, ivKey);
                        encryptedCardNumber += "|" + ivKey;
                    }
                    repoPaymentTransaction.UpdatePaymentTransaction(transactionNumber, null, bankName,
                        result.Payload, authCode, transCode, responseCode, cardNumber: encryptedCardNumber, cardMethod: cardMethod,
                        cardType: cardType, expiryMonth: expiryMonth, expiryYear: expiryYear, appId: appId, responseCode: respCode,
                        errorMessage: errorMessage, newTransactionId: newTransactionId);

                    var paymentTransaction = repoPaymentTransaction.GetPaymentTransactionByTransactionNumber(transactionNumber);
                    result.OrderNumber = paymentTransaction.OrderNumber;
                    result.Amount = paymentTransaction.Amount;
                    result.MerchantId = paymentTransaction.MerchantId;
                    result.ReturnUrl = paymentTransaction.ReturnUrl;
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
                using (var repoPaymentTransaction = new PaymentTransactionRepository())
                {
                    repoPaymentTransaction.UpdatePaymentTransaction(transactionNumber, null, null, null, null, null, PaymentResponseCode.UserCancel);
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
            PaymentTransaction paymentTransaction = null;
            using (var svcPaymentTrans = new PaymentTransactionService())
            {
                var paymentProvideType = (PaymentProviderType)model.PaymentProviderType;
                paymentTransaction = svcPaymentTrans.GetPaymentTransactionByOrderNumber(model.OrderNo, paymentProvideType.ToString());
            }
            if (paymentTransaction == null)
            {
                outputModel.Code = ResponseReturnCode.Gen_RecordNotFound;
                outputModel.Message = "Invalid transaction number";

            }
            else
            {
                outputModel.Code = ResponseReturnCode.Gen_Success;
                outputModel.Amt = string.Format("{0:.00}", paymentTransaction.Amount);
                outputModel.Status = paymentTransaction.TransactionStatusId.ToString();
                outputModel.OrderNo = paymentTransaction.OrderNumber;
                outputModel.RefNo = paymentTransaction.PaymentReferenceNumber;
                outputModel.AuthCode = paymentTransaction.AuthorizationCode;
                outputModel.AuthNo = paymentTransaction.AuthorizationNumber;
                outputModel.SellerId = paymentTransaction.MerchantId;
                outputModel.CreatedOn = paymentTransaction.CreatedOn != null ? paymentTransaction.CreatedOn.ToString() : string.Empty;
                outputModel.CardMethod = paymentTransaction.CardMethod;
                outputModel.CardType = paymentTransaction.CardType;
                outputModel.ResponseCode = paymentTransaction.ResponseCode;
                outputModel.ErrorMessage = paymentTransaction.ErrorMessage;
                outputModel.ExpiryMonth = paymentTransaction.ExpiryMonth;
                outputModel.ExpiryYear = paymentTransaction.ExpiryYear;
                outputModel.IsDifferentRenewalMethod = paymentTransaction.IsDifferentRenewalMethod;
                outputModel.IsEnrolment = paymentTransaction.IsEnrolment.ToString();
                outputModel.IsInitialPayment = paymentTransaction.IsInitialPayment.ToString();
                outputModel.TransactionCode = paymentTransaction.TransactionNumber;
                outputModel.Bank = paymentTransaction.Bank;
                outputModel.CardNumber = paymentTransaction.CardNumber;
                outputModel.PaymentRef = paymentTransaction.ProposalId;
                outputModel.NewTransactionCode = paymentTransaction.NewTransactionId;
            }

            //if (outputModel.ResponseCode != "00")
            //{
            //    //call to database to get latest status
            //    try
            //    {



            //    }
            //    catch (Exception ex)
            //    {
            //        LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);

            //        outputModel.Code = ResponseReturnCode.Gen_InternalServerError;
            //        outputModel.Message = ex.ToString();
            //    }
            //}

            //if (!string.IsNullOrEmpty(paymentTransaction.CardNumber))
            //{
            //    var encryptedCardNumber = paymentTransaction.CardNumber;

            //    var splitData = encryptedCardNumber.Split('|');
            //    var ivKey = splitData[1];
            //    var cardNumber = AESMethod.DecryptString(splitData[0], ivKey);

            //    //masked it
            //    //return first and last 4
            //    var maskedSection = cardNumber.Length - 8;
            //    for (int i = 0; i < cardNumber.Length; i++)
            //    {
            //        if (i <= 3 || i > cardNumber.Length - 5)
            //            result.CardNumber += cardNumber[i];
            //        else
            //            result.CardNumber += "X";
            //    }
            //}

            return outputModel;
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
