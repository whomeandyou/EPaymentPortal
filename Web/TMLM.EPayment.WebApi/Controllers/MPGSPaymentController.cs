using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;
using TMLM.Common;
using TMLM.EPayment.BL.Data.MPGSPayment;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Gateway;
using TMLM.EPayment.BL.PaymentProvider;
using TMLM.EPayment.BL.PaymentProvider.MPGS;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.WebApi;
using TMLM.EPayment.WebApi.Controllers;
using TMLM.EPayment.WebApi.ViewModels;
using TMLM.EPayment.Db.Tables;
using Newtonsoft.Json.Linq;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.Helpers;
using TMLM.Security.Crytography;
using System.Diagnostics;
using System.Web;
using TMLM.EPayment.Db.Repositories;
using System.Threading.Tasks;

namespace TMLM.EForm.Web.Controllers
{
    public class MPGSPaymentController : BaseController
    {
        // Post: MPGSPayment
        [HttpPost, Route("MPGSPayment/Request")]
        public ActionResult Index(GatewayApiRequest gatewayApiRequest)
        {
            ResponseToMerchant responseToMerchantOut = new ResponseToMerchant();
            string mpgsMerchantId;
            string mpgsUsername;
            string mpgsMerchantPassword;

            try
            {
                LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(gatewayApiRequest,
                                                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                if (gatewayApiRequest.IsInitialPayment)
                {
                    //hash verification 
                    var hashAuthentication = new HashAuthentication(); 
                    if (!hashAuthentication.CompareRequestSignature(gatewayApiRequest.Amt, gatewayApiRequest.OrderNo, gatewayApiRequest.AId,
                        gatewayApiRequest.MsgSign))
                    {
                        return View("~/Views/Error/Index.cshtml", new ErrorVM()
                        {
                            Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                            Description = "Invalid Signature"
                        });
                    }
                }

                var mpgsUtilities = new MPGSUtilities();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                getMerchantInfo(out mpgsUsername, out mpgsMerchantId, out mpgsMerchantPassword, gatewayApiRequest.AId);

                //ResponseToMerchant responseToMerchanttest = IdUtils.returnMessage(true, gatewayApiRequest,responseToMerchantOut);
                //return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchanttest);

                gatewayApiRequest.TransactionId = IdUtils.generateSampleId();
                //gatewayApiRequest.TransactionReference = IdUtils.generateSampleId();
                gatewayApiRequest.OrderCurrency = MPGSSettings.CURRENCY;
                gatewayApiRequest.SecureId = IdUtils.generateSampleId();
                gatewayApiRequest.GatewayApiConfig = new GatewayApiConfig();
                gatewayApiRequest.GatewayApiConfig.Password = mpgsMerchantPassword;
                gatewayApiRequest.GatewayApiConfig.Username = mpgsUsername;
                gatewayApiRequest.GatewayApiConfig.MerchantId = mpgsMerchantId;
                gatewayApiRequest.GatewayApiConfig.Version = MPGSSettings.VERSION;
                gatewayApiRequest.ApiOperation = MPGSSettings.CREATE_SESSION;
                gatewayApiRequest.GatewayApiConfig.GatewayUrl = MPGSSettings.BASE_URL;
                
                if(gatewayApiRequest.IsInitialPayment)
                {
                    gatewayApiRequest.Operation = "PURCHASE";
                }
                else
                {
                    gatewayApiRequest.Operation = "VERIFY";
                }

                //testing
                // gatewayApiRequest.OrderId = IdUtils.generateSampleId();
                gatewayApiRequest.OrderReference = gatewayApiRequest.OrderNo;
                ModelState.Remove("OrderNo");
                gatewayApiRequest.OrderNo = IdUtils.generateEnrolmentOrderId(gatewayApiRequest); //"-E"
               
                var svcPayment = new PaymentService();
                InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
                {
                    OrderNo = gatewayApiRequest.OrderNo,
                    PaymentProviderType = (int)PaymentProviderType.MPGS
                };
                
                var inquiryResult = svcPayment.InquiryPayment(inquiryPaymentInputModel);

                if (SuccessfulMapping(inquiryResult.ErrorMessage))
                {
                    responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                    responseToMerchantOut.AuthCode = inquiryResult.AuthCode;
                   
                    responseToMerchantOut.CardMethod = inquiryResult.CardMethod;
                    responseToMerchantOut.CardType = inquiryResult.CardType;

                    responseToMerchantOut.CCNumber = inquiryResult.CardNumber;

                    if (string.IsNullOrEmpty(inquiryResult.Bank))
                    {
                         string CCNumber =  Utils.decryptCCNumber(inquiryResult.CardNumber);

                        if (CCNumber.Length >= 6)
                        {
                            responseToMerchantOut.BankName = getBankNameForService(inquiryResult.Bank, CCNumber);
                        }
                    }
                    else
                    {
                        responseToMerchantOut.BankName = inquiryResult.Bank;
                    }

                    responseToMerchantOut.ErrorMessage = inquiryResult.ErrorMessage;
                    responseToMerchantOut.ExpiryMonth = inquiryResult.ExpiryMonth;
                    responseToMerchantOut.ExpiryYear = inquiryResult.ExpiryYear;
                    responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                    responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                    responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                    responseToMerchantOut.ResponseCode = inquiryResult.ResponseCode;
                    responseToMerchantOut.TransCode = inquiryResult.TransactionCode;
                    responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?)null;

                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(true, gatewayApiRequest, responseToMerchantOut);
                    return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
                }
                else
                {   
                    if (!string.IsNullOrEmpty(inquiryResult.CardNumber))
                    {
                        string retrieveTransactionReponse;
                        if (inquiryResult.NewTransactionCode != null)
                        {
                             retrieveTransactionReponse = mpgsUtilities.InquiryMPGSPayment(gatewayApiRequest, inquiryResult.OrderNo, inquiryResult.NewTransactionCode);
                        }
                        else
                        {
                             retrieveTransactionReponse = mpgsUtilities.InquiryMPGSPayment(gatewayApiRequest, inquiryResult.OrderNo, inquiryResult.TransactionCode);
                        }
                        
                        PayEnrollmentResponseModel ResponseModel = null;
                        if (retrieveTransactionReponse != null)
                        {
                            JObject jObject = JObject.Parse(retrieveTransactionReponse);
                            if (jObject["error"] == null && jObject["transaction"]["type"] != null)
                            {
                                string transactionType = jObject["transaction"]["type"].ToString();
                                if (transactionType == "VERIFICATION")
                                {
                                    ResponseModel = PayEnrollmentResponseModel.toVerifyResponseModel(retrieveTransactionReponse, out responseToMerchantOut);
                                }
                                else if (transactionType == "PAYMENT")
                                {
                                    ResponseModel = PayEnrollmentResponseModel.toPayResponseModel(retrieveTransactionReponse, out responseToMerchantOut);
                                }

                                if (string.IsNullOrEmpty(responseToMerchantOut.BankName))
                                {
                                    string CCNumber = Utils.decryptCCNumber(inquiryResult.CardNumber);

                                    if (CCNumber.Length >= 6)
                                    {
                                        responseToMerchantOut.BankName = getBankNameForService(responseToMerchantOut.BankName, CCNumber);
                                    }

                                    if(string.IsNullOrEmpty(responseToMerchantOut.BankName))
                                    {
                                        responseToMerchantOut.BankName = inquiryResult.Bank;
                                    }
                                }
                                else
                                {
                                    responseToMerchantOut.BankName = inquiryResult.Bank;
                                }
                            }

                            
                        }

                        if (SuccessfulMapping(responseToMerchantOut.ErrorMessage))
                        {
                            responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                            responseToMerchantOut.CCNumber = inquiryResult.CardNumber;
                            responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                            responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                            responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                            responseToMerchantOut.PaymentRef = inquiryResult.PaymentRef;
                            responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?)null;
                            ResponseToMerchant responseToMerchant = IdUtils.returnMessage(true, gatewayApiRequest, responseToMerchantOut);
                            return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
                        }

                    }
                }
          

                var initiateResult = svcPayment.InitiatePayment(new InitiatePaymentInputModel() 
                {
                    ApplicationAccountCode = gatewayApiRequest.AId,
                    Amount = decimal.Parse(gatewayApiRequest.Amt),
                    Currency = gatewayApiRequest.OrderCurrency,
                    PaymentProviderId = (int)PaymentProviderType.MPGS,
                    TransactionNumber = gatewayApiRequest.TransactionId,
                    OrderNumber = gatewayApiRequest.OrderNo,
                    CreatedOn = DateTime.Now,
                    ReturnUrl = gatewayApiRequest.ReturnUrl,
                    AdditionalInfo = gatewayApiRequest.OrderDescription,
                    IsEnrolment = gatewayApiRequest.IsEnrolment,
                    IsInitialPayment = gatewayApiRequest.IsInitialPayment,
                    PaymentRef = gatewayApiRequest.PaymentRef
                });

                if (initiateResult.Code != ResponseReturnCode.Gen_Success)
                {
                    responseToMerchantOut.ErrorMessage = initiateResult.Message;
                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut);
                    return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
                }
                
                var checkOutSessionResponse = mpgsUtilities.CreateCheckOutSession(gatewayApiRequest);

                CheckoutSessionModel checkoutSessionModel = CheckoutSessionModel.toCheckoutSessionModel(
                    checkOutSessionResponse, out responseToMerchantOut);

                if (string.IsNullOrEmpty(checkoutSessionModel.Id))
                {
                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut);
                    return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
                }
                gatewayApiRequest.MonthDropdownlist = IdUtils.dropdownlistMonth();
                gatewayApiRequest.YearDropdownlist = IdUtils.dropdownlistYear();

                svcPayment.UpdatePaymentInformation(new UpdatePaymentInformationInputModel()
                {
                    TransactionNumber = gatewayApiRequest.TransactionId,
                    IsDifferentRenewalMethod = gatewayApiRequest.IsDifferentRenewalMethod,
                    ProposalId = gatewayApiRequest.PaymentRef,
                    SecureId = gatewayApiRequest.SecureId,
                    SessionId = checkoutSessionModel.Id
                });

                return View(gatewayApiRequest);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("Request Errror : " + ex.ToString() );
                responseToMerchantOut.ErrorMessage = ex.Message;
                ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut);
                return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
            }
        }


        [HttpPost, Route("MPGSPayment/Inquiry")]
        public ActionResult inquiry(GatewayApiRequest gatewayApiRequest)
        {
            ResponseToMerchant responseToMerchantOut = new ResponseToMerchant();
            ResponseToMerchant responseToMerchant = new ResponseToMerchant();
            responseToMerchant.IsSuccess = false;
            string mpgsMerchantId;
            string mpgsUsername;
            string mpgsMerchantPassword;

            try
            {
                LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(gatewayApiRequest,
                                                         new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                var mpgsUtilities = new MPGSUtilities();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                getMerchantInfo(out mpgsUsername, out mpgsMerchantId, out mpgsMerchantPassword, gatewayApiRequest.AId);

                gatewayApiRequest.OrderCurrency = MPGSSettings.CURRENCY;
                gatewayApiRequest.GatewayApiConfig = new GatewayApiConfig();
                gatewayApiRequest.GatewayApiConfig.Password = mpgsMerchantPassword;
                gatewayApiRequest.GatewayApiConfig.Username = mpgsUsername;
                gatewayApiRequest.GatewayApiConfig.MerchantId = mpgsMerchantId;
                gatewayApiRequest.GatewayApiConfig.Version = MPGSSettings.VERSION;
                gatewayApiRequest.GatewayApiConfig.GatewayUrl = MPGSSettings.BASE_URL;

                gatewayApiRequest.OrderReference = gatewayApiRequest.OrderNo;
                ModelState.Remove("OrderNo");
                gatewayApiRequest.OrderNo = IdUtils.generateEnrolmentOrderId(gatewayApiRequest);

                var svcPayment = new PaymentService();
                InquiryPaymentInputModel inquiryPaymentInputModel = new InquiryPaymentInputModel()
                {
                    OrderNo = gatewayApiRequest.OrderNo,
                    PaymentProviderType = (int)PaymentProviderType.MPGS
                };
                var inquiryResult = svcPayment.InquiryPayment(inquiryPaymentInputModel);

                if (SuccessfulMapping(inquiryResult.ErrorMessage))
                {
                    responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                    responseToMerchantOut.AuthCode = inquiryResult.AuthCode;
                    responseToMerchantOut.BankName = inquiryResult.Bank;
                    responseToMerchantOut.CardMethod = inquiryResult.CardMethod;
                    responseToMerchantOut.CardType = inquiryResult.CardType;

                    responseToMerchantOut.CCNumber = inquiryResult.CardNumber;
                    responseToMerchantOut.ErrorMessage = inquiryResult.ErrorMessage;
                    responseToMerchantOut.ExpiryMonth = inquiryResult.ExpiryMonth;
                    responseToMerchantOut.ExpiryYear = inquiryResult.ExpiryYear;
                    responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                    responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                    responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                    responseToMerchantOut.ResponseCode = inquiryResult.ResponseCode;
                    responseToMerchantOut.TransCode = inquiryResult.TransactionCode;
                    responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?)null;
                    responseToMerchant.PaymentRef = inquiryResult.PaymentRef;
                    responseToMerchant = IdUtils.returnMessage(true, gatewayApiRequest, responseToMerchantOut, true);
                }
                else if(!string.IsNullOrEmpty(inquiryResult.ResponseCode))
                {
                    responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                    responseToMerchantOut.AuthCode = inquiryResult.AuthCode;
                    responseToMerchantOut.BankName = inquiryResult.Bank;
                    responseToMerchantOut.CardMethod = inquiryResult.CardMethod;
                    responseToMerchantOut.CardType = inquiryResult.CardType;

                    responseToMerchantOut.CCNumber = inquiryResult.CardNumber;
                    responseToMerchantOut.ErrorMessage = inquiryResult.ErrorMessage;
                    responseToMerchantOut.ExpiryMonth = inquiryResult.ExpiryMonth;
                    responseToMerchantOut.ExpiryYear = inquiryResult.ExpiryYear;
                    responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                    responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                    responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                    responseToMerchantOut.ResponseCode = inquiryResult.ResponseCode;
                    responseToMerchantOut.TransCode = inquiryResult.TransactionCode;
                    responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?)null;
                    responseToMerchant.PaymentRef = inquiryResult.PaymentRef;
                    responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut, true);
                }
                else
                {
                    if (!string.IsNullOrEmpty(inquiryResult.CardNumber))
                    {

                        string retrieveTransactionReponse;
                        if (inquiryResult.NewTransactionCode != null)
                        {
                            retrieveTransactionReponse = mpgsUtilities.InquiryMPGSPayment(gatewayApiRequest, inquiryResult.OrderNo, inquiryResult.NewTransactionCode);
                        }
                        else
                        {
                            retrieveTransactionReponse = mpgsUtilities.InquiryMPGSPayment(gatewayApiRequest, inquiryResult.OrderNo, inquiryResult.TransactionCode);
                        }
                        PayEnrollmentResponseModel ResponseModel = null;
                        if (retrieveTransactionReponse != null)
                        {
                            JObject jObject = JObject.Parse(retrieveTransactionReponse);
                            if (jObject["error"] == null && jObject["transaction"]["type"] != null)
                            {
                                string transactionType = jObject["transaction"]["type"].ToString();
                                if (transactionType == "VERIFICATION")
                                {
                                    ResponseModel = PayEnrollmentResponseModel.toVerifyResponseModel(retrieveTransactionReponse, out responseToMerchantOut);
                                }
                                else if (transactionType == "PAYMENT")
                                {
                                    ResponseModel = PayEnrollmentResponseModel.toPayResponseModel(retrieveTransactionReponse, out responseToMerchantOut);
                                }
                            }
                        }

                        responseToMerchantOut.Veres = null;
                        responseToMerchantOut.Pares = null;
                        responseToMerchantOut.AcsEci = null;
                        responseToMerchantOut.TransactionId = null;
                        responseToMerchantOut.AuthenticationToken = null;
                        responseToMerchantOut.dsVersion = null;


                        if (SuccessfulMapping(responseToMerchantOut.ErrorMessage))
                        {
                            responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                            responseToMerchantOut.CCNumber = inquiryResult.CardNumber;
                            responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                            responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                            responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                            responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?) null;
                            responseToMerchant.PaymentRef = inquiryResult.PaymentRef;
                            responseToMerchant = IdUtils.returnMessage(true, gatewayApiRequest, responseToMerchantOut,true);
                        }
                        else
                        {
                            responseToMerchantOut.AmountPaid = inquiryResult.Amt;
                            responseToMerchantOut.AuthCode = inquiryResult.AuthCode;
                            responseToMerchantOut.BankName = inquiryResult.Bank;
                            responseToMerchantOut.CardMethod = inquiryResult.CardMethod;
                            responseToMerchantOut.CardType = inquiryResult.CardType;

                            responseToMerchantOut.CCNumber = inquiryResult.CardNumber;
                            responseToMerchantOut.ErrorMessage = inquiryResult.ErrorMessage;
                            responseToMerchantOut.ExpiryMonth = inquiryResult.ExpiryMonth;
                            responseToMerchantOut.ExpiryYear = inquiryResult.ExpiryYear;
                            responseToMerchantOut.IsDifferentRenewalMethod = inquiryResult.IsDifferentRenewalMethod;
                            responseToMerchantOut.IsInitialPayment = inquiryResult.IsInitialPayment;
                            responseToMerchantOut.IsEnrolment = inquiryResult.IsEnrolment;
                            responseToMerchantOut.ResponseCode = inquiryResult.ResponseCode;
                            responseToMerchantOut.TransCode = inquiryResult.TransactionCode;
                            responseToMerchantOut.PaymentDate = inquiryResult.CreatedOn != null ? Convert.ToDateTime(inquiryResult.CreatedOn) : (DateTime?)null;
                            responseToMerchant.PaymentRef = inquiryResult.PaymentRef;
                            responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut, true);
                        }
                    }
                }
                if(responseToMerchant.IsSuccess)
                {
                    if (string.IsNullOrEmpty(responseToMerchant.BankName))
                    {
                        string CCNumber = Utils.decryptCCNumber(responseToMerchant.CCNumber);

                        if (CCNumber.Length >= 6)
                        {
                            responseToMerchant.BankName = getBankNameForService(responseToMerchant.BankName, CCNumber);
                        }
                    }
                }
                return Json(responseToMerchant);
            }
            catch (Exception ex)
            {
                responseToMerchantOut.ErrorMessage = ex.Message;
                responseToMerchant = IdUtils.returnMessage(false, gatewayApiRequest, responseToMerchantOut, true);
                return Json(responseToMerchant);
            }
        }


        [HttpPost, Route("MPGSPayment/ProcessTransaction")]
        public ActionResult ProcessTransaction(GatewayApiRequest model)
        {
            ResponseToMerchant responseToMerchantOut = new ResponseToMerchant();
            string mpgsUsername;
            string mpgsMerchantId;
            string mpgsMerchantPassword;
            
            try
            {
                //create payment session for pass value 
                var mpgsUtilities = new MPGSUtilities();

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var svcPaymentTransaction = new PaymentTransactionService();
                PaymentTransaction paymentTransactionModel =
                       svcPaymentTransaction.GetPaymentInformationByOrderIdAndTransactioNum(model.OrderNo, model.TransactionId);

                GetPaymentInforOutputModel getPaymentInforOutputModel =
                    IdUtils.getPaymentOutput(paymentTransactionModel);

                getMerchantInfo(out mpgsUsername, out mpgsMerchantId, out mpgsMerchantPassword, model.AId);

                model.TransactionId = getPaymentInforOutputModel.TransactionCode;
                model.GatewayApiConfig = new GatewayApiConfig();
                model.GatewayApiConfig.Password = mpgsMerchantPassword;
                model.GatewayApiConfig.Username = mpgsUsername;
                model.GatewayApiConfig.MerchantId = mpgsMerchantId;
                model.GatewayApiConfig.Version = MPGSSettings.VERSION;
                model.ApiOperation = MPGSSettings.API_OPERATION;
                model.GatewayApiConfig.GatewayUrl = MPGSSettings.BASE_URL;
                model.SecureId = getPaymentInforOutputModel.SecureID;
                model.SessionId = getPaymentInforOutputModel.SessionID;
                model.InitiateAuthentication = true;
               
                //string payResponse1  = MasterCardPaymentVerifyMethod(model);
                model.OrderReference = string.Empty;
                var responseEnrol = mpgsUtilities.Check3dsEnrollment(model);

                SecureIdEnrollmentResponseModel modelEnrol = SecureIdEnrollmentResponseModel.toSecureIdEnrollmentResponseModel(responseEnrol, out responseToMerchantOut);

                //if (modelEnrol.VeResEnrolled != "Y" && modelEnrol.VeResEnrolled != "N")
                //{
                //    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, responseToMerchantOut);
                //    return Json(redirect(responseToMerchant));
                //}

                //testing
                model.OrderReference = model.OrderNo;
                model.TransactionReference = model.TransactionId;

                if (modelEnrol.AuthenticationStatus == "AUTHENTICATION_AVAILABLE")
                {
                    model.InitiateAuthentication = false;
                    model.PayerAuthenticate = false;
                }
                else if(modelEnrol.GatewayRecommendation == "DO_NOT_PROCEED" || string.IsNullOrEmpty(modelEnrol.GatewayRecommendation))
                {
                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, responseToMerchantOut);
                    return Json(redirect(responseToMerchant));
                }

                Task.Delay(5000).Wait();
 
                var updateSessionResponse = mpgsUtilities.UpdateSession(model);
                CheckoutSessionModel checkoutSessionModel = CheckoutSessionModel.toCheckoutSessionModel(updateSessionResponse, out responseToMerchantOut);

                if (string.IsNullOrEmpty(checkoutSessionModel.Id))
                {
                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, responseToMerchantOut);
                    return Json(redirect(responseToMerchant));
                }

                string authenticationStatus = modelEnrol.AuthenticationStatus;

                Task.Delay(5000).Wait();
                //PAYER_AUTHENTICATION
                if (modelEnrol.AuthenticationStatus == "AUTHENTICATION_AVAILABLE")
                {
                    model.ApiOperation = MPGSSettings.API_OPERATION_PAYER_AUTHENTICATE;
                    model.InitiateAuthentication = false;
                    model.PayerAuthenticate = true;
                    model.SessionId = checkoutSessionModel.Id;
                    var responsePayerAuthenticate = mpgsUtilities.Payer3dsAuthentication(model);
                    modelEnrol = SecureIdEnrollmentResponseModel.PayerAuthenticateResponseModel(responsePayerAuthenticate, out responseToMerchantOut);
                }

                if (modelEnrol.GatewayRecommendation == "PROCEED")
                { 
                    model.PayerAuthenticate = false;
                }
                

                if (model.CardNumber.Length >= 6)
                {
                    BankService bankService = new BankService();
                    MPGSBinBankList mPGSBinBankList = bankService.GetMPGSBinBankNameList(model.CardNumber.Substring(0, 6));
                    if (mPGSBinBankList != null && !string.IsNullOrEmpty(mPGSBinBankList.Bin))
                    {
                        model.BankName = mPGSBinBankList.BankName;
                    }
                }

                var svcPayment = new PaymentService();
                var formValues = new Dictionary<string, string>();
                formValues.Add("TransactionNumber", getPaymentInforOutputModel.TransactionCode);
                formValues.Add("CardNumber", model.CardNumber);
                formValues.Add("ExpiryMonth", model.ExpiryMonth);
                formValues.Add("ExpiryYear", model.ExpiryYear);
                formValues.Add("BankName", model.BankName);
                var paymentResult = svcPayment.ProcessPayment(new ProcessPaymentInputModel()
                {
                    Form = formValues,
                    PaymentProviderId = (int)PaymentProviderType.MPGS
                });

                //if payer authenticate oni otp
                if (modelEnrol.GatewayRecommendation == "PROCEED" &&
                    authenticationStatus == "AUTHENTICATION_AVAILABLE")
                {
                    modelEnrol.returnURL = model.SecureIdResponseUrl;
                    modelEnrol.MdValue = model.OrderNo + "|" + model.TransactionId;
                    //int startIndex = responseEnrol.IndexOf("<form name");
                    //int endIndex = responseEnrol.IndexOf("</body>");
                    //string subres = responseEnrol.Substring(0, endIndex);
                    //subres = subres.Substring(startIndex, subres.Length - startIndex);
                    //subres = subres.Replace("\\", "");

                    return Json(redirectOTP(modelEnrol));
                }
                else if (modelEnrol.GatewayRecommendation == "PROCEED"
                     && authenticationStatus != "AUTHENTICATION_AVAILABLE")
                {
                    getPaymentInforOutputModel.CardNumber = model.CardNumber;
                    getPaymentInforOutputModel.ExpiryMonth = model.ExpiryMonth;
                    getPaymentInforOutputModel.ExpiryYear = model.ExpiryYear;

                    ResponseToMerchant responseToMerchant = ResponseToMerchantMessage(string.Empty, getPaymentInforOutputModel);

                    if (string.IsNullOrEmpty(responseToMerchant.BankName))
                    {
                        string CCNumber = model.CardNumber;

                        if (CCNumber.Length >= 6)
                        {
                            responseToMerchant.BankName = getBankNameForService(model.BankName, CCNumber);
                        }
                    }
                    else
                    {
                        responseToMerchant.BankName = model.BankName;
                    }
                    if(string.IsNullOrEmpty(responseToMerchant.ExpiryYear) || string.IsNullOrEmpty(responseToMerchant.ExpiryMonth))
                    {
                        responseToMerchant.ExpiryYear = model.ExpiryYear;
                        responseToMerchant.ExpiryMonth = model.ExpiryMonth;
                    }
                    //process payment
                    var paymentResultNotEnrol = ProcessPayment(getPaymentInforOutputModel.TransactionCode, responseToMerchant);
                    responseToMerchant.CCNumber = paymentResultNotEnrol.CardNumber;
                    if (paymentResultNotEnrol.Code != ResponseReturnCode.Gen_Success)
                    {
                        responseToMerchant.ErrorMessage = "Database Error";
                    }
                    return Json(redirect(responseToMerchant));
                }
                else
                {
                    responseToMerchantOut.ErrorMessage = modelEnrol.gatewayCode;
                    ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, responseToMerchantOut);
                    return Json(redirect(responseToMerchant));
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("ProcessTransaction Error : " + ex.ToString());
                responseToMerchantOut.ErrorMessage = ex.Message;
                ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, responseToMerchantOut);
                return Json(redirect(responseToMerchant));
            }

        }

        [HttpPost, Route("MPGSPayment/getBankName")]
        public ActionResult getBankName(GetBankNameRequestModel model)
        {
            GetBankNameResponseModel getBankNameResponseModel = new GetBankNameResponseModel();
            try
            {
                BankService bankService = new BankService();
                if (model.CCNumber.Length >= 6)
                {
                    MPGSBinBankList mPGSBinBankList = bankService.GetMPGSBinBankNameList(model.CCNumber.Substring(0, 6));
                    if (mPGSBinBankList != null && !string.IsNullOrEmpty(mPGSBinBankList.Bin))
                    {
                        getBankNameResponseModel.bankName = mPGSBinBankList.BankName;
                        getBankNameResponseModel.bin = mPGSBinBankList.Bin;
                    }
                }
                return Json(getBankNameResponseModel);
                
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("getBankName Error : " + ex.ToString());
             
                return Json(getBankNameResponseModel);
            }

        }

        [HttpGet]
        [ActionName("ApiResponse")]
        //[HttpGet("ApiResponse")]
        public ActionResult PaymentResponse()
        {
            GatewayApiRequest gatewayApiRequest = GatewayApiRequest.createApiRequest(GatewayApiConfig);

            gatewayApiRequest.GatewayApiConfig = GatewayApiConfig;
            gatewayApiRequest.buildRequestUrl();
            gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            buildViewData(gatewayApiRequest, response);

            //return View(ViewList.GetValueOrDefault("ApiResponse"));
            return View();
        }

        [HttpPost, Route("MPGSPayment/PaymentResult")]
        public ActionResult PaymentResult(ResponseToMerchant responseToMerchant)
        {
            return View(responseToMerchant);
        }

        //private string getSessionJsUrl(GatewayApiConfig gatewayApiConfig)
        //{
        //    return $@"{GatewayApiConfig.GatewayUrl}/form/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/session.js";
        //}

        [HttpPost, Route("MPGSPayment/CancelTransaction")]
        public ActionResult CancelTransaction(GatewayApiRequest model)
        {
            var svcPayment = new PaymentService();
            var cancelResult = svcPayment.CancelPayment(model.TransactionId);

            ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, null);
            responseToMerchant.ErrorMessage = "Cancel transaction";

            return Json(redirect(responseToMerchant));
        }

        public ActionResult RedirectTransaction(GatewayApiRequest model)
        {
            ResponseToMerchant responseToMerchant = IdUtils.returnMessage(false, model, null);
            responseToMerchant.ErrorMessage = "Time out";

            return Json(redirect(responseToMerchant));
        }

        [HttpPost, Route("MPGSPayment/ProccessRedirect")]
        public ActionResult ProccessRedirect()
        {
            var PaRes = Request.Form["PARes"];
            var transactionId = Request.Form["transaction.id"];
            var orderId = Request.Form["order.id"];
            var svcPayment = new PaymentService();
            var gateway = Request.Form["response.gatewayRecommendation"];
            var result = Request.Form["result"];
            
            var svcPaymentTransaction = new PaymentTransactionService();
            PaymentTransaction paymentTransactionModel =
                   svcPaymentTransaction.GetPaymentInformationByOrderIdAndTransactioNum(orderId, transactionId);

            GetPaymentInforOutputModel getPaymentInforOutputModel =
                IdUtils.getPaymentOutput(paymentTransactionModel);

            //var repoApplicationAccount = new ApplicationAccountRepository();
            //var applicationAccount = repoApplicationAccount.GetApplicationAccountById(paymentTransactionModel.)

            if (gateway == "DO_NOT_PROCEED")
            {
                ResponseToMerchant responseToMerchants = new ResponseToMerchant
                {
                    AmountPaid = getPaymentInforOutputModel.Amt,
                    IsSuccess = false,
                    TransCode = getPaymentInforOutputModel.TransactionCode,
                    AppId = getPaymentInforOutputModel.ApplicationAccountCode,
                    AuthCode = getPaymentInforOutputModel.AuthCode,
                    BankName = getPaymentInforOutputModel.Bank,
                    CardMethod = getPaymentInforOutputModel.CardMethod,
                    CardType = getPaymentInforOutputModel.CardType,
                    CCNumber = getPaymentInforOutputModel.CardNumber,
                    ExpiryMonth = getPaymentInforOutputModel.ExpiryMonth,
                    ExpiryYear = getPaymentInforOutputModel.ExpiryYear,
                    ResponseCode = getPaymentInforOutputModel.ResponseCode,
                    ErrorMessage = getPaymentInforOutputModel.ErrorMessage,
                    IsEnrolment = getPaymentInforOutputModel.IsEnrolment,
                    IsInitialPayment = getPaymentInforOutputModel.IsInitialPayment,
                    IsDifferentRenewalMethod = getPaymentInforOutputModel.IsDifferentRenewalMethod,
                    PaymentRef = getPaymentInforOutputModel.proposalID?.ToString(),
                    msgSign = getPaymentInforOutputModel.MsgSign,
                    ReturnURL = getPaymentInforOutputModel.returnURL
                                        
                };
                if (!string.IsNullOrEmpty(getPaymentInforOutputModel.OrderNo))
                {
                    responseToMerchants.AppId = getPaymentInforOutputModel.IsInitialPayment == "True" ? getPaymentInforOutputModel.OrderNo : getPaymentInforOutputModel.OrderNo.Substring(0, getPaymentInforOutputModel.OrderNo.Length - 2);
                }
                return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchants);
            }

            
            var responseToMerchant = ResponseToMerchantMessage(PaRes,getPaymentInforOutputModel);

            var paymentResult = ProcessPayment(getPaymentInforOutputModel.TransactionCode, responseToMerchant);

            bool InitialPayment = Convert.ToBoolean(responseToMerchant.IsInitialPayment);
            //if (responseToMerchant.IsSuccess && InitialPayment)
            //{
            //    try
            //    {
            //        svcPaymentTransaction.InsertEnrollmentInformation(new EnrollmentInformationModel()
            //        {
            //            PaymentTransactionId = paymentTransactionModel.Id,
            //            Veres = responseToMerchant.Veres,
            //            Pares = responseToMerchant.Pares,
            //            AcsEci = responseToMerchant.AcsEci,
            //            AuthenticationToken = responseToMerchant.AuthenticationToken,
            //            TransactionId = responseToMerchant.TransactionId,
            //            dsVersion = responseToMerchant.dsVersion
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);
            //        responseToMerchant.IsSuccess = false;
            //        responseToMerchant.ErrorMessage = ex.Message;
            //        return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
            //    }
            //}

            if (paymentResult.Code != ResponseReturnCode.Gen_Success)
            {
                return View("~/Views/Error/Index.cshtml", new ErrorVM()
                {
                    Code = TMLM.Common.ResponseReturnCode.Gen_Error_Occur,
                    Description = "Invalid callback " + paymentResult.Message
                });
            }

            return View("~/Views/MPGSPayment/PaymentResult.cshtml", responseToMerchant);
        }

        private ProcessPaymentOutputModel ProcessPayment(string transactionCode, ResponseToMerchant responseToMerchant)
        {

            var formValues = new Dictionary<string, string>();
            ////assign first before its getting removed in ResponseToMerchantMessage
            formValues.Add("TransactionNumber", transactionCode);
            formValues.Add("ErrorMessage", responseToMerchant.ErrorMessage);
            formValues.Add("CardNumber", responseToMerchant.CCNumber);
            formValues.Add("ExpiryMonth", responseToMerchant.ExpiryMonth);
            formValues.Add("ExpiryYear", responseToMerchant.ExpiryYear);
            formValues.Add("CardType", responseToMerchant.CardType);
            formValues.Add("CardMethod", responseToMerchant.CardMethod);
            formValues.Add("TransCode", responseToMerchant.TransCode);
            formValues.Add("AppId", responseToMerchant.AppId);
            formValues.Add("BankName", responseToMerchant.BankName);
            formValues.Add("AuthCode", responseToMerchant.AuthCode);
            formValues.Add("ResponseCode", responseToMerchant.ResponseCode);
            formValues.Add("newTransactionId", responseToMerchant.newTransactionId);

            var svcPayment = new PaymentService();
            var paymentResult = svcPayment.ProcessPayment(new ProcessPaymentInputModel()
            {
                Form = formValues,
                PaymentProviderId = (int)PaymentProviderType.MPGS
            });
            string encryptedCardNumber = responseToMerchant.CCNumber;
            if (!string.IsNullOrEmpty(encryptedCardNumber) && encryptedCardNumber.Length < 20)
            {
                var ivKey = BitConverter.ToString(Encoding.ASCII.GetBytes(RandomHelper.RandomString(16))).Replace("-", string.Empty);
                encryptedCardNumber = AESMethod.EncryptString(responseToMerchant.CCNumber, ivKey);
                encryptedCardNumber += "|" + ivKey;
            }
            paymentResult.CardNumber = encryptedCardNumber;
            return paymentResult;
        }


        public ResponseToMerchant ResponseToMerchantMessage(string PaRes, GetPaymentInforOutputModel getPaymentModel)
        {
            string mpgsUsername, mpgsMerchantId, mpgsMerchantPassword;
            getMerchantInfo(out mpgsUsername, out mpgsMerchantId, out mpgsMerchantPassword, getPaymentModel.ApplicationAccountCode);

            var gatewayApiConfig = new GatewayApiConfig();
            gatewayApiConfig.Password = mpgsMerchantPassword;
            gatewayApiConfig.Username = mpgsUsername;
            gatewayApiConfig.MerchantId = mpgsMerchantId;
            gatewayApiConfig.Version = MPGSSettings.VERSION;
            gatewayApiConfig.GatewayUrl = MPGSSettings.BASE_URL;

            GatewayApiRequest acsModel = new GatewayApiRequest(gatewayApiConfig);
            GatewayApiRequest payModel = new GatewayApiRequest(gatewayApiConfig);
            ResponseToMerchant responseToMerchant = new ResponseToMerchant();
            ResponseToMerchant responseToMerchantOut = new ResponseToMerchant();
            SecureIdEnrollmentResponseModel EnrollmentResponseModel = null;
            var svcPaymentTransaction = new PaymentTransactionService();
            try
            {
                bool isInitialPayment = Convert.ToBoolean(getPaymentModel.IsInitialPayment);

                payModel.IsDifferentRenewalMethod = getPaymentModel.IsDifferentRenewalMethod;
                payModel.IsInitialPayment = isInitialPayment;

                payModel.Amt = getPaymentModel.Amt;
                payModel.OrderCurrency = getPaymentModel.Currency;
                payModel.TransactionId = getPaymentModel.TransactionCode;
                payModel.OrderNo = getPaymentModel.OrderNo;
                payModel.ReturnUrl = getPaymentModel.returnURL;
                payModel.PaymentRef = getPaymentModel.proposalID?.ToString();
                payModel.ApplicationAccountCode = getPaymentModel.ApplicationAccountCode;
                //payModel.CardNumber = Utils.decryptCCNumber(getPaymentModel.CardNumber);
                //payModel.ExpiryMonth = getPaymentModel.ExpiryMonth;
                //payModel.ExpiryYear = getPaymentModel.ExpiryYear;
                payModel.OrderReference = getPaymentModel.OrderNo;
                payModel.TransactionReference = getPaymentModel.OrderNo;
                payModel.SessionId = getPaymentModel.SessionID;

                string CCNumbers = "";
                if (getPaymentModel.CardNumber.Length >= 17)
                {
                    CCNumbers = Utils.decryptCCNumber(getPaymentModel.CardNumber);
                }
                //payModel.agreementId = String.Format("{0}{1}{2}{3}", getPaymentModel.OrderNo, GetLast4Digits(CCNumbers), getPaymentModel.ExpiryYear, getPaymentModel.ExpiryMonth);

                var mpgsUtilities = new MPGSUtilities();
                var enrollmentResponse = mpgsUtilities.ReceiveTransaction(payModel);
                EnrollmentResponseModel = SecureIdEnrollmentResponseModel.EnrollmentResponseModel(enrollmentResponse, out responseToMerchantOut);

                if (EnrollmentResponseModel.version == "3DS2")
                {
                    payModel.acseci = EnrollmentResponseModel.acsEci?.Trim();
                    payModel.dsTransactionId = EnrollmentResponseModel.transactionId?.Trim();
                    payModel.transactionStatus = EnrollmentResponseModel.transactionStatus?.Trim();
                    payModel.statusReasonCode = EnrollmentResponseModel.statusReasonCode?.Trim();
                    payModel.authenticationToken = EnrollmentResponseModel.authenticationToken?.Trim();
                    payModel.dsVersion = EnrollmentResponseModel.version?.Trim();
                }
                else if (EnrollmentResponseModel.version == "3DS1")
                {
                    payModel.acseci = EnrollmentResponseModel.acsEci?.Trim();
                    payModel.dsTransactionId = EnrollmentResponseModel.transactionId?.Trim();
                    payModel.authenticationToken = EnrollmentResponseModel.authenticationToken?.Trim();
                    payModel.veres = EnrollmentResponseModel.VeResEnrolled?.Trim();
                    payModel.pares = EnrollmentResponseModel.pares?.Trim();
                    payModel.dsVersion = EnrollmentResponseModel.version?.Trim();
                }
                else
                {
                    payModel.ExpiryMonth = getPaymentModel.ExpiryMonth;
                    payModel.ExpiryYear = getPaymentModel.ExpiryYear;
                }

                string payResponse = string.Empty;
                PayEnrollmentResponseModel VerifyEnrollmentResponseModel = null;
                PayEnrollmentResponseModel PayEnrollmentResponseModel = null;

             
                MPGSUtilities mPGSUtilities = new MPGSUtilities();
                if (!isInitialPayment)
                {
                    //try
                    //{
                    //    svcPaymentTransaction.InsertEnrollmentInformation(new EnrollmentInformationModel()
                    //    {
                    //        PaymentTransactionId = getPaymentModel.Id,
                    //        Veres = payModel.veres,
                    //        Pares = payModel.pares,
                    //        AcsEci = payModel.acseci,
                    //        AuthenticationToken = payModel.authenticationToken,
                    //        TransactionId = payModel.dsTransactionId,
                    //        dsVersion = payModel.dsVersion
                    //    });
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogManager.GetLogger(this.GetType()).Error(ex.Message, ex);
                    //    ResponseToMerchant responseToMerchants = new ResponseToMerchant
                    //    {
                    //        AmountPaid = getPaymentModel.Amt,
                    //        IsSuccess = false,
                    //        TransCode = getPaymentModel.TransactionCode,
                    //        AppId = getPaymentModel.ApplicationAccountCode,
                    //        AuthCode = getPaymentModel.AuthCode,
                    //        BankName = getPaymentModel.Bank,
                    //        CardMethod = getPaymentModel.CardMethod,
                    //        CardType = getPaymentModel.CardType,
                    //        CCNumber = Utils.decryptCCNumber(getPaymentModel.CardNumber),
                    //        ExpiryMonth = getPaymentModel.ExpiryMonth,
                    //        ExpiryYear = getPaymentModel.ExpiryYear,
                    //        ResponseCode = getPaymentModel.ResponseCode,
                    //        ErrorMessage = ex.Message,
                    //        IsEnrolment = getPaymentModel.IsEnrolment,
                    //        IsInitialPayment = getPaymentModel.IsInitialPayment,
                    //        IsDifferentRenewalMethod = getPaymentModel.IsDifferentRenewalMethod,
                    //        PaymentRef = getPaymentModel.proposalID?.ToString(),
                    //        msgSign = getPaymentModel.MsgSign,
                    //        ReturnURL = getPaymentModel.returnURL
                    //    };
                    //    return responseToMerchants;
                    //}

                    payResponse = mPGSUtilities.MasterCardPaymentVerifyMethod(payModel);
                    VerifyEnrollmentResponseModel = PayEnrollmentResponseModel.toVerifyResponseModel(payResponse, out responseToMerchantOut);

                }
                else
                {
                    payResponse = mPGSUtilities.MasterCardPaymentPayMethod(payModel);
                    PayEnrollmentResponseModel = PayEnrollmentResponseModel.toPayResponseModel(payResponse, out responseToMerchantOut);
                }

                responseToMerchant = IdUtils.returnMessage(false, payModel, responseToMerchantOut);

                //if success
                if (SuccessfulMapping(responseToMerchantOut.ErrorMessage))
                {
                    responseToMerchant.IsSuccess = true;
                    if (string.IsNullOrEmpty(responseToMerchantOut.BankName))
                    {
                        responseToMerchant.newTransactionId = payModel.newTransactionId;
                        if (getPaymentModel.CardNumber.Length >= 17)
                        {
                            string CCNumber = Utils.decryptCCNumber(getPaymentModel.CardNumber);
                            responseToMerchant.BankName = getBankNameForService(getPaymentModel.Bank, CCNumber);
                        }
                    }
                }

                responseToMerchant.CCNumber = getPaymentModel.CardNumber;

                return responseToMerchant;
            }
            catch (Exception ex)
            {
                responseToMerchantOut.ErrorMessage = ex.ToString();
                responseToMerchant = IdUtils.returnMessage(false, payModel, responseToMerchantOut);
                return responseToMerchant;
            }
        }

        [HttpPost, Route("MPGSPayment/Inquiry3dsAuthentication")]
        public ActionResult Inquiry3dsAuthentication(Inquiry3dsAuthenticationInfoRequestModel requestModel)
        {
            LogManager.GetLogger(this.GetType()).Info("RequestView : " + JsonConvert.SerializeObject(requestModel,
                                                       new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            var mpgsUtilities = new MPGSUtilities();
            var svcPaymentTransaction = new PaymentTransactionService();
            ResponseToMerchant responseToMerchantOut = new ResponseToMerchant();
            string mpgsUsername, mpgsMerchantId, mpgsMerchantPassword;
            var inquiry3dsResponse = new Inquiry3dsAuthenticationInfoResponseModel();

            try
            {
                var paymentTransaction = svcPaymentTransaction.GetPaymentTransactionByOrderNumberSuccessful(requestModel.orderNo);
                
                if(paymentTransaction == null)
                {
                    inquiry3dsResponse.isSuccess = false;
                    inquiry3dsResponse.errorMessage = "Invalid Order Number";
                    return Json(inquiry3dsResponse);
                }
                getMerchantInfo(out mpgsUsername, out mpgsMerchantId, out mpgsMerchantPassword, paymentTransaction.ApplicationAccountCode);
                
                var apiConfig = new GatewayApiConfig();
                apiConfig.GatewayUrl = MPGSSettings.BASE_URL;
                apiConfig.Version = MPGSSettings.VERSION;
                apiConfig.MerchantId = mpgsMerchantId;
                apiConfig.Password = mpgsMerchantPassword;
                apiConfig.Username = mpgsUsername;

                var gatewayrequest = new GatewayApiRequest(apiConfig);
                gatewayrequest.OrderNo = paymentTransaction.OrderNumber;
                if (!string.IsNullOrEmpty(paymentTransaction.NewTransactionId))
                {
                    gatewayrequest.TransactionId = paymentTransaction.NewTransactionId;
                }
                else
                {
                    gatewayrequest.TransactionId = paymentTransaction.TransactionNumber;
                }

                var response = mpgsUtilities.ReceiveTransaction(gatewayrequest);
                var EnrollmentResponseModel = SecureIdEnrollmentResponseModel.EnrollmentResponseModel(response, out responseToMerchantOut);

                if (responseToMerchantOut.ErrorMessage != null)
                {
                    inquiry3dsResponse.isSuccess = false;
                    inquiry3dsResponse.errorMessage = responseToMerchantOut.ErrorMessage;
                    return Json(inquiry3dsResponse);
                }

                var enrollmentInfo = svcPaymentTransaction.GetEnrollmentInformation(paymentTransaction.Id);
                if(enrollmentInfo != null)
                {
                    EnrollmentInformationModel updateEnroll = new EnrollmentInformationModel()
                    {
                        AcsEci = EnrollmentResponseModel.acsEci,
                        AuthenticationToken = EnrollmentResponseModel.authenticationToken,
                        Veres = EnrollmentResponseModel.VeResEnrolled,
                        Pares = EnrollmentResponseModel.pares,
                        TransactionId = EnrollmentResponseModel.transactionId,
                        PaymentTransactionId = paymentTransaction.Id,
                        CreditCardNumber = EnrollmentResponseModel.CreditCardNumber,
                        OrderNumber = EnrollmentResponseModel.orderNumber,
                        StatusReasonCode = EnrollmentResponseModel.statusReasonCode,
                        dsVersion = EnrollmentResponseModel.version,
                        TransactionStatus = EnrollmentResponseModel.transactionStatus,
                        ReceiveTransactionID = gatewayrequest.TransactionId
                    };
                    svcPaymentTransaction.UpdateEnrollmentInformation(updateEnroll);
                }
                else
                {
                    svcPaymentTransaction.InsertEnrollmentInformation(new EnrollmentInformationModel()
                    {
                        PaymentTransactionId = paymentTransaction.Id,
                        Veres = EnrollmentResponseModel.VeResEnrolled,
                        Pares = EnrollmentResponseModel.pares,
                        AcsEci = EnrollmentResponseModel.acsEci,
                        AuthenticationToken = EnrollmentResponseModel.authenticationToken,
                        TransactionId = EnrollmentResponseModel.transactionId,
                        dsVersion = EnrollmentResponseModel.version,
                        CreditCardNumber = EnrollmentResponseModel.CreditCardNumber,
                        OrderNumber = EnrollmentResponseModel.orderNumber,
                        StatusReasonCode = EnrollmentResponseModel.statusReasonCode,
                        TransactionStatus = EnrollmentResponseModel.transactionStatus,
                        ReceiveTransactionID = gatewayrequest.TransactionId
                    });
                }

                inquiry3dsResponse.isSuccess = true;
                return Json(inquiry3dsResponse);
            }
            catch(Exception ex)
            {
                inquiry3dsResponse.isSuccess = false;
                inquiry3dsResponse.errorMessage = ex.Message;
                return Json(inquiry3dsResponse);
            }
        }

        private string VoidPaymentPayMethod(GatewayApiRequest requestModel, PayEnrollmentResponseModel payEnrollmentResponseModel)
        {
            GatewayApiRequest model = new GatewayApiRequest();
            model.GatewayApiConfig = new GatewayApiConfig();
            //model.ApiMethod = GatewayApiClient.PUT;
            model.ApiOperation = MPGSSettings.API_OPERATION_VOID;
            model.GatewayApiConfig.GatewayUrl = requestModel.GatewayApiConfig.GatewayUrl;
            model.GatewayApiConfig.MerchantId = requestModel.GatewayApiConfig.MerchantId;
            model.GatewayApiConfig.Version = requestModel.GatewayApiConfig.Version;
            model.GatewayApiConfig.Username = requestModel.GatewayApiConfig.Username;
            model.GatewayApiConfig.Password = requestModel.GatewayApiConfig.Password;

            model.OrderNo = requestModel.OrderNo;
            model.TransactionId = payEnrollmentResponseModel.transactionID;

            //requestModel.TransactionId;
            model.TargetTransactionId = requestModel.TransactionId;
            model.TransactionReference = IdUtils.generateSampleId();
            //requestModel.TransactionId;

            model.buildPayload();
            model.buildRequestUrl();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.PUT;
            apiRequest.RequestUrl = model.RequestUrl;
            apiRequest.Payload = model.Payload;
            apiRequest.Password = model.GatewayApiConfig.Password;
            apiRequest.Username = model.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var payResponse = apiClient.SendTransaction(apiRequest);


            return payResponse;
        }

        public string redirect(ResponseToMerchant model)
        {
            StringBuilder form = new StringBuilder();
            form.Append("<form name=\"echoForm\" method=\"POST\" action=\"" + model.ReturnURL + "\">");
            form.Append("<div id = \"msg\">");
            form.Append("<input type=\"hidden\" name=\"AmountPaid\" value=\"" + model.AmountPaid + "\">");
            form.Append("<input type=\"hidden\" name=\"IsSuccess\" value=\"" + model.IsSuccess.ToString() + "\">");
            form.Append("<input type=\"hidden\" name=\"TransCode\" value=\"" + model.TransCode + "\">");
            form.Append("<input type=\"hidden\" name=\"AppId\" value=\"" + model.AppId + "\">");
            form.Append("<input type=\"hidden\" name=\"AuthCode\" value=\"" + model.AuthCode + "\">");
            form.Append("<input type=\"hidden\" name=\"BankName\" value=\"" + model.BankName + "\">");
            form.Append("<input type=\"hidden\" name=\"CardMethod\" value=\"" + model.CardMethod + "\">");
            form.Append("<input type=\"hidden\" name=\"CardType\" value=\"" + model.CardType + "\">");
            form.Append("<input type=\"hidden\" name=\"CCNumber\" value=\"" + model.CCNumber + "\">");
            form.Append("<input type=\"hidden\" name=\"ExpiryMonth\" value=\"" + model.ExpiryMonth + "\">");
            form.Append("<input type=\"hidden\" name=\"ExpiryYear\" value=\"" + model.ExpiryYear + "\">");
            form.Append("<input type=\"hidden\" name=\"ResponseCode\" value=\"" + model.ResponseCode + "\">");
            form.Append("<input type=\"hidden\" name=\"ErrorMessage\" value=\"" + model.ErrorMessage + "\">");
            form.Append("<input type=\"hidden\" name=\"IsEnrolment\" value=\"" + model.IsEnrolment + "\">");
            form.Append("<input type=\"hidden\" name=\"IsDifferentRenewalMethod\" value=\"" + model.IsDifferentRenewalMethod.ToString() + "\">");
            form.Append("<input type=\"hidden\" name=\"isInitialPayment\" value=\"" + model.IsInitialPayment + "\">");
            form.Append("<input type=\"hidden\" name=\"PaymentRef\" value=\"" + model.PaymentRef + "\">");
            form.Append("<input type=\"hidden\" name=\"msgSign\" value=\"" + model.msgSign + "\">");
            form.Append("</div></form>");

            return form.ToString();
        }


        public string redirectOTP(SecureIdEnrollmentResponseModel model)
        {
            StringBuilder form = new StringBuilder();
            form.Append("<form name=\"echoForm\" method=\"POST\" action=\"" + model.AcsUrl + "\">");
            form.Append("<div id = \"msg\">");
            form.Append("<input type=\"hidden\" name=\"TermUrl\" value=\"" + model.termURL + "\">");
            form.Append("<input type=\"hidden\" name=\"PaReq\" value=\"" + model.Pareq + "\">");
            form.Append("<input type=\"hidden\" name=\"creq\" value=\"" + model.CReq + "\">");
            form.Append("<input type=\"hidden\" name=\"MD\" value=\"" + model.MdValue + "\">");
            form.Append("</div></form>");
            return form.ToString();
        }

        private bool SuccessfulMapping(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                return false;
            }

            List<string> SuccessList = new List<string>
            {
                "APPROVED","APPROVED_AUTO"
            };

            if(SuccessList.Contains(message.Trim().ToUpper()))
            {
                return true;
            }

            return false;
        }

        private string getBankNameForService(string bankname,string CCNumber)
        {
            string bankName = string.Empty;
            BankService bankService = new BankService();
            MPGSBinBankList mPGSBinBankList = bankService.GetMPGSBinBankNameList(CCNumber.Substring(0, 6));
            if (mPGSBinBankList != null)
            {
                if (!string.IsNullOrEmpty(mPGSBinBankList.Bin))
                {
                    bankName = mPGSBinBankList.BankName;
                }
                else if (!string.IsNullOrEmpty(bankname))
                {
                    bankName = bankname;
                }
                else
                {
                    bankName = mPGSBinBankList.BankName;
                }
            }

            return bankname;
        }

        public static string GetLast4Digits(string accountNumber)
        {
            int strLength = 4;
            if (String.IsNullOrEmpty(accountNumber))
                return string.Empty;

            if (strLength >= accountNumber.Length)
                return accountNumber;

            return accountNumber.Substring(accountNumber.Length - strLength);
        }

        private void getMerchantInfo(out string merchantUsername, out string merchantId, out string merchantPassword, string aid)
        {
            var repoApplicationAccount = new ApplicationAccountService();
            var applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(aid);

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                merchantUsername = oRSA.Decrypt(privateKey, applicationAccount.MerchantUserName);
                CAcert = null;
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                merchantId = oRSA.Decrypt(privateKey, applicationAccount.Merchant);
                CAcert = null;
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                merchantPassword = oRSA.Decrypt(privateKey, applicationAccount.MerchantPassword);
                CAcert = null;
            }
        }
    }
}