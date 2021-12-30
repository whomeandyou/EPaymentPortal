using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMLM.BL.Data;
using TMLM.EPayment.BL.Data.MPGSPayment;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.PaymentProvider.MPGS
{
    public class IdUtils
    {

        public static string generateSampleId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);
        }

        public static List<DropDownListModel> dropdownlistMonth()
        {
            List<DropDownListModel> dropDownListModel = new List<DropDownListModel>();

            dropDownListModel = Enumerable
          .Range(1, 12).Select(i => new DropDownListModel
          {
              Id = i.ToString().PadLeft(2, '0'),
              Value = DateTime.ParseExact(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
              "MMMM", CultureInfo.InvariantCulture).Month.ToString().PadLeft(2, '0')
          }).ToList();

            return dropDownListModel;
        }

        public static List<DropDownListModel> dropdownlistYear()
        {
            List<DropDownListModel> dropDownListModel = new List<DropDownListModel>();
            int year = Convert.ToInt16(DateTime.Now.Year.ToString().Substring(2));
            dropDownListModel = Enumerable
          .Range(year, 15).Select(i => new DropDownListModel
          {
              Id = i.ToString(),
              Value = i.ToString()
          }).ToList();

            return dropDownListModel;
        }

        public static string generateEnrolmentOrderId(GatewayApiRequest gatewayApiRequest)
        {
            if (!gatewayApiRequest.IsInitialPayment)
            {
                gatewayApiRequest.OrderNo = gatewayApiRequest.OrderNo + "-E";
            }
            return gatewayApiRequest.OrderNo;
        }

        public static ResponseToMerchant returnMessage(bool isSuccess, GatewayApiRequest model,
            ResponseToMerchant response,bool isInquiry = false)
        {
            if (response == null)
            {
                response = new ResponseToMerchant();
            }
            
            ResponseToMerchant responseToMerchant = response;
            responseToMerchant.IsSuccess = isSuccess;
            responseToMerchant.ReturnURL = model.ReturnUrl;
            if (isInquiry)
            {
                responseToMerchant.AmountPaid = response.AmountPaid;
            }
            else
            {
                responseToMerchant.AmountPaid = model.Amt;
            }
            if (!string.IsNullOrEmpty(model.OrderNo))
            {
                responseToMerchant.AppId = model.IsInitialPayment ? model.OrderNo : model.OrderNo.Substring(0, model.OrderNo.Length - 2);
            }
            responseToMerchant.ResponseCode = response.ResponseCode;
            responseToMerchant.TransCode = response.TransCode;
            responseToMerchant.ErrorMessage = response.ErrorMessage;
            if (!string.IsNullOrEmpty(model.PaymentRef))
            {
                responseToMerchant.PaymentRef = model.PaymentRef;
            }
            else
            {
                responseToMerchant.PaymentRef = response.PaymentRef;
            }
            if (!string.IsNullOrEmpty(response.IsEnrolment))
            {
                responseToMerchant.IsEnrolment = response.IsEnrolment;
                responseToMerchant.IsDifferentRenewalMethod = response.IsDifferentRenewalMethod;
                responseToMerchant.IsInitialPayment = response.IsInitialPayment;
            }
            else
            {
                responseToMerchant.IsEnrolment = !model.IsInitialPayment ? "true" : "false";
                responseToMerchant.IsDifferentRenewalMethod = model.IsDifferentRenewalMethod;
                responseToMerchant.IsInitialPayment = model.IsInitialPayment ? "true" : "false";
            }


            if (responseToMerchant.IsSuccess)
            {
                responseToMerchant.AuthCode = response.AuthCode;
                responseToMerchant.BankName = response.BankName;
                responseToMerchant.CardMethod = response.CardMethod;
                responseToMerchant.CardType = response.CardType;
                responseToMerchant.CCNumber = response.CCNumber;
                responseToMerchant.ExpiryMonth = response.ExpiryMonth;
                responseToMerchant.ExpiryYear = response.ExpiryYear;
                responseToMerchant.PaymentDate = response.PaymentDate;
            }

            //Foo - comment it since not assign the msgsign value
            //string aid = string.Empty;

            //if(!string.IsNullOrEmpty(model.ApplicationAccountCode))
            //{
            //    aid = model.ApplicationAccountCode;
            //}
            //else if(string.IsNullOrEmpty(model.AId))
            //{
            //    using (var repoPaymentTransaction = new PaymentTransactionRepository())
            //    {
            //        var paymentTransaction = repoPaymentTransaction.GetPaymentTransactionByOrderNumber(model.OrderNo, PaymentProviderType.MPGS.ToString());
            //        aid = paymentTransaction.ApplicationAccountCode;
            //    }
            //}
            //else
            //{
            //    aid = model.AId;
            //}

            //var hashAuthentication = new HashAuthentication();
            //var msgSign = hashAuthentication.GetResponseSignature(string.Format("{0:.00}", responseToMerchant.AmountPaid), responseToMerchant.OrderNo,
            //        0, aid, responseToMerchant.CCNumber);

            return responseToMerchant;
        }

        public static GetPaymentInforOutputModel getPaymentOutput(PaymentTransaction model)
        {
            GetPaymentInforOutputModel outputmodel = new GetPaymentInforOutputModel()
            {
                Id = model.Id,
                Amt = string.Format("{0:.00}", model.Amount),
                Status = model.TransactionStatusId.ToString(),
                OrderNo = model.OrderNumber,
                RefNo = model.PaymentReferenceNumber,
                AuthCode = model.AuthorizationCode,
                AuthNo = model.AuthorizationNumber,
                SellerId = model.MerchantId,
                CreatedOn = model.CreatedOn.ToString("dd MMM yyyy hh:mm:ss tt"),
                CardMethod = model.CardMethod,
                CardType = model.CardType,
                ResponseCode = model.ResponseCode,
                ErrorMessage = model.ErrorMessage,
                ExpiryMonth = model.ExpiryMonth,
                ExpiryYear = model.ExpiryYear,
                IsDifferentRenewalMethod = model.IsDifferentRenewalMethod,
                IsEnrolment = model.IsEnrolment.ToString(),
                IsInitialPayment = model.IsInitialPayment.ToString(),
                TransactionCode = model.TransactionNumber,
                Bank = model.Bank,
                CardNumber = model.CardNumber,
                SessionID = model.SessionId,
                SecureID = model.SecureId,
                Currency = model.Currency,
                returnURL = model.ReturnUrl,
                proposalID = model.ProposalId,
                ApplicationAccountCode = model.ApplicationAccountCode
            };

            return outputmodel;
        }

        public static string convertCCNumberToText(string encryptedCardNum)
        {
            string[] splitCardString = null;
            if (!string.IsNullOrEmpty(encryptedCardNum))
            {
                splitCardString = encryptedCardNum.Split('|');
            }

            if(splitCardString.Count() == 1)
            {
                return encryptedCardNum;
            }

            string cardNumberString = string.Empty;
            if (splitCardString.Count() > 1 && !string.IsNullOrEmpty(splitCardString[1].ToString()))
            {
                cardNumberString = AESMethod.DecryptString(splitCardString[0].ToString(), splitCardString[1].ToString());
            }

            return cardNumberString;
        }
    }
}

