using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Data.RazerPay;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.Service.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.PaymentProvider.RazerPay
{
    public class RazerPayProcessor : IPaymentProcessor
    {
        bool disposed = false;

        public OutputModel CancelPayment(string transactionNumber)
        {
            throw new NotImplementedException();
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

        public OutputModel FailPaymentWithStatus(string transactionNumber, string status)
        {
            throw new NotImplementedException();
        }

        public GetHtmlOutputModel GenerateRequestHTML(GetHtmlInputModel model)
        {
            var result = new GetHtmlOutputModel();
            var nameHtml = string.Empty;
            var emailHtml = string.Empty;
            var mobileHtml = string.Empty;
            var descriptionHtml = string.Empty;
            var paymentTransaction = new PaymentTransaction();
            var razerPayUtilities = new RazerPayUtilities();
            RazerPayDecryptOutputModel razerPayOutputModel = new RazerPayDecryptOutputModel();

            using (var repoPayment = new PaymentTransactionService())
                paymentTransaction = repoPayment.GetPaymentTransactionByTransactionNumber(model.TransactionNumber);

            ApplicationAccount applicationAccount = null;
            using (var repoPayment = new ApplicationAccountService())
                applicationAccount = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);

            razerPayOutputModel = DecryptFPXHepler.RazerPayDecrypt(applicationAccount.RazerPayPublicKey, applicationAccount.RazerPayPrivateKey, applicationAccount.RazerPayMerchantId);

            #region Buyer HTML
            if (!string.IsNullOrEmpty(paymentTransaction.BuyerName))
                nameHtml = $"<input type='hidden' name='bill_name' value='{paymentTransaction.BuyerName}' />";

            if (!string.IsNullOrEmpty(paymentTransaction.BuyerEmail))
                emailHtml = $"<input type='hidden' name='bill_email' value='{paymentTransaction.BuyerEmail}' />";

            if (!string.IsNullOrEmpty(paymentTransaction.BuyerContact))
                mobileHtml = $"<input type='hidden' name='bill_mobile' value='{paymentTransaction.BuyerContact}' />";

            if (!string.IsNullOrEmpty(paymentTransaction.Description))
                descriptionHtml = $"<input type='hidden' name='bill_desc' value='{paymentTransaction.Description}' />";
            #endregion

            var hash = razerPayUtilities.hashPaymentRequest(paymentTransaction.Amount.ToString(), razerPayOutputModel.MerchantId, paymentTransaction.OrderNumber, razerPayOutputModel.PublicKey);


            try
            {
                result.FormHtml = $@"
<form action='{RazerPaySettings.BASE_URL}{razerPayOutputModel.MerchantId}/' method='post'>
    <input type='hidden' name='amount' value='{paymentTransaction.Amount}' />
    <input type='hidden' name='orderid' value='{paymentTransaction.OrderNumber}' />
    {nameHtml}
    {emailHtml}
    {mobileHtml}
    {descriptionHtml}
    <input type='hidden' name='country' value='MY' />
    <input type='hidden' name='cancelurl' value='{paymentTransaction.CancelUrl}' />
    <input type='hidden' name='vcode' value='{hash}' />
</form>
";
                using (var repo = new RazerPayLogRepository())
                {
                    string temp = result.FormHtml;
                    var tempResult = temp.Replace(hash, "<CheckSum>");

                    repo.InsertRazerPayLog(paymentTransaction.OrderNumber, $"{RazerPaySettings.BASE_URL}{razerPayOutputModel.MerchantId}/", tempResult, "");
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

        public OutputModel InitiatePayment(InitiatePaymentInputModel model)
        {
            var result = new OutputModel();
            RazerPayDecryptOutputModel razerPayOutputModel = new RazerPayDecryptOutputModel();

            try
            {
                ApplicationAccount applicationAccount;
                using (var repoApplicationAccount = new ApplicationAccountRepository())
                    applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(model.ApplicationAccountCode);

                //insert into db 
                using (var repoPaymentTransaction = new PaymentTransactionRepository())
                {
                    repoPaymentTransaction.InsertPaymentTransaction(model.TransactionNumber, model.Currency, model.Amount,
                        model.OrderNumber, applicationAccount.Id, model.ReturnUrl, razerPayOutputModel.MerchantId,
                        PaymentProviderType.RazerPay.ToString(), isEnrolment: model.IsEnrolment,
                        isInitialPayment: model.IsInitialPayment, buyerName: model.BuyerName, buyerEmail: model.BuyerEmail, buyerContact: model.MobilePhoneNo, cancelUrl: model.CancelUrl);
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
                outputModel.NewTransactionCode = paymentTransaction.NewTransactionId;
                outputModel.Bank = paymentTransaction.Bank;
                outputModel.CardNumber = paymentTransaction.CardNumber;
                outputModel.PaymentRef = paymentTransaction.ProposalId;
            }
            return outputModel;
        }

        public ProcessPaymentOutputModel ProcessPayment(ProcessPaymentInputModel model)
        {
            var result = new ProcessPaymentOutputModel();
            PaymentResponseOutputModel outputModel = new PaymentResponseOutputModel();
            var utilities = new RazerPayUtilities();
            PaymentTransaction paymentTransactionInfo = null;
            ApplicationAccount applicationAccountInfo = new ApplicationAccount();

            try
            {
                #region Assign Form Data to Object
                outputModel.Amount = model.Form["amount"];
                outputModel.OrderNo = model.Form["orderid"];
                outputModel.RazerPayTransactionNo = model.Form["tranID"];
                outputModel.Domain = model.Form["domain"];
                outputModel.Status = model.Form["status"];
                outputModel.AppCode = model.Form["appcode"];
                outputModel.ErrorCode = model.Form["error_code"];
                outputModel.ErrorDescription = model.Form["error_desc"];
                outputModel.SKey = model.Form["skey"];
                outputModel.Currency = model.Form["currency"];
                outputModel.Channel = model.Form["channel"];
                outputModel.PayDate = model.Form["paydate"];
                if (model.Form.ContainsKey("extraP"))
                    outputModel.ExtraParam = model.Form["extraP"];
                #endregion

                var responseS = JsonConvert.SerializeObject(model.Form);

                using (var repoPayment = new PaymentTransactionRepository())
                    paymentTransactionInfo = repoPayment.GetPaymentTransactionByOrderNumber(outputModel.OrderNo, PaymentProviderType.RazerPay.ToString());

                using (var repoPayment = new ApplicationAccountService())
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransactionInfo.ApplicationAccountId);


                //update db
                var responseCode = PaymentResponseCode.Failed;
                if (outputModel.Status == "00")
                    responseCode = PaymentResponseCode.Success;

                using (var repoPayment = new PaymentTransactionRepository())
                {
                    repoPayment.UpdatePaymentTransaction(paymentTransactionInfo.TransactionNumber, outputModel.RazerPayTransactionNo,
                                                        string.Empty, responseS, outputModel.Status, string.Empty, responseCode,
                                                        errorMessage: outputModel.ErrorDescription, responseCode: outputModel.ErrorCode, channel: outputModel.Channel);
                }

                result.Amount = decimal.Parse(outputModel.Amount);
                result.OrderNumber = outputModel.OrderNo;
                result.PaymentResponseCode = outputModel.Status;
                result.Code = ResponseReturnCode.Gen_Success;

                using (var repo = new RazerPayLogRepository())
                {
                    model.Form["skey"] = "<skey>";
                    string temp = new JavaScriptSerializer().Serialize(model.Form);

                    repo.InsertRazerPayLog(outputModel.OrderNo, $"{RazerPaySettings.BASE_URL}{outputModel.Domain}/","" , temp);
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
    }
}
