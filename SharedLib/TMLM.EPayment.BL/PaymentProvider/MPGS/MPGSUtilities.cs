using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.BL.Gateway;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.PaymentProvider.MPGS
{
    public class MPGSUtilities
    {
        public string ProcessACSResult(GatewayApiRequest gatewayApiRequest, string paRes, string orderNo)
        {
            // Retrieve session
            gatewayApiRequest.PaymentAuthResponse = paRes;
            gatewayApiRequest.buildPayload();
            gatewayApiRequest.buildSecureIdRequestUrl();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(orderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + orderNo + " " + ex.Message, ex.InnerException);
            }

            return response;

        }

        public string CreateCheckOutSession(GatewayApiRequest gatewayApiRequest)
        {
            gatewayApiRequest.buildSessionRequestUrl();
            gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Password = "<password>";
                    apiRequest.Username = "<username>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch(Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " "+ ex.Message, ex.InnerException);
            }
            return response;
        }


        public string UpdateSession(GatewayApiRequest gatewayApiRequest)
        {
            gatewayApiRequest.ApiOperation = string.Empty;
            gatewayApiRequest.buildSessionRequestUrl();
            gatewayApiRequest.SessionId = string.Empty;
            if(!string.IsNullOrEmpty(gatewayApiRequest.PayorName))
            {
                if(gatewayApiRequest.PayorName.Length <= 50)
                {
                    gatewayApiRequest.CustomerLastName = gatewayApiRequest.PayorName;
                }
                else
                {
                    gatewayApiRequest.CustomerLastName = gatewayApiRequest.PayorName.Substring(0,50);
                }
            }
          
            gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.PUT;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    JObject root = JObject.Parse(apiRequest.Payload);
                    JObject item = (JObject)root["sourceOfFunds"]["provided"]["card"];
                    var ivKey = BitConverter.ToString(Encoding.ASCII.GetBytes(RandomHelper.RandomString(16))).Replace("-", string.Empty);
                    string encryptedCardNumber = AESMethod.EncryptString(item["number"].ToString(), ivKey);
                    encryptedCardNumber += "|" + ivKey;
                    item["number"] = encryptedCardNumber;
                    item.Property("number").AddAfterSelf(new JProperty("new", "New value"));
                    apiRequest.Payload = root.ToString();
                    
                    apiRequest.Password = "<password>";
                    apiRequest.Username = "<username>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }


        public string Check3dsEnrollment(GatewayApiRequest gatewayApiRequest)
        {
            // Retrieve session
            gatewayApiRequest.buildRequestUrl();
            gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.PUT;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);


            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    JObject root = JObject.Parse(apiRequest.Payload);
                    JObject item = (JObject)root["sourceOfFunds"]["provided"]["card"];
                    item["number"] = "<cardNumber>";

                    item.Property("number").AddAfterSelf(new JProperty("new", "New value"));
                    apiRequest.Payload = root.ToString();
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }

        public string Payer3dsAuthentication(GatewayApiRequest gatewayApiRequest)
        {
            // Retrieve session
            gatewayApiRequest.buildRequestUrl();
            gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.PUT;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    JObject root = JObject.Parse(apiRequest.Payload);
                    apiRequest.Payload = root.ToString();
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }

        public string ReceiveTransaction(GatewayApiRequest gatewayApiRequest)
        {
            // Retrieve session
            gatewayApiRequest.buildRequestUrl();
            //gatewayApiRequest.buildPayload();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.GET;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            //apiRequest.Payload = gatewayApiRequest.Payload;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, "ReceiveTransaction", request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }

        public string InquiryMPGSPayment(GatewayApiRequest gatewayApiRequest,string orderId,string transactionId)
        {
            // Retrieve transaction
            gatewayApiRequest.buildRetrieveTransactionRequestUrl(orderId,transactionId);

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.GET;
            apiRequest.RequestUrl = gatewayApiRequest.RequestUrl;
            apiRequest.Password = gatewayApiRequest.GatewayApiConfig.Password;
            apiRequest.Username = gatewayApiRequest.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(gatewayApiRequest.OrderNo, gatewayApiRequest.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + gatewayApiRequest.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }


        public string MasterCardPaymentVerifyMethod(GatewayApiRequest model)
        {
            model.ApiOperation = MPGSSettings.API_OPERATION_VERIFY;
            model.PaymentAuthResponse = string.Empty;
            model.SecureIdResponseUrl = string.Empty;
            model.SourceType = "CARD";
            model.TransactionId = IdUtils.generateSampleId();
            model.newTransactionId = model.TransactionId;
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
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(model.OrderNo, model.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + model.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }

        public string MasterCardPaymentPayMethod(GatewayApiRequest model)
        {
            model.ApiOperation = MPGSSettings.API_OPERATION_PAY;
            model.PaymentAuthResponse = string.Empty;

            model.SecureId3D = model.SecureId;
            model.SourceType = "CARD";
            if (model.ApplicationAccountCode == "ESUB")
            {
                model.statementDescriptorName = "TOKIO MARINE LIFE ECOM " + model.OrderNo;
            }
            else
            {
                model.statementDescriptorName = "TOKIO MARINE LIFE ECOM";
            }
            model.buildPayload();
            model.TransactionId = IdUtils.generateSampleId();
            model.newTransactionId = model.TransactionId;
            model.buildRequestUrl();

            //standardize to use api client call
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.PUT;
            apiRequest.RequestUrl = model.RequestUrl;
            apiRequest.Payload = model.Payload;
            apiRequest.Password = model.GatewayApiConfig.Password;
            apiRequest.Username = model.GatewayApiConfig.Username;

            var apiClient = new ApiClient();
            var response = apiClient.SendTransaction(apiRequest);

            try
            {
                using (var mpgsLogTransaction = new MPGSLogRepository())
                {
                    apiRequest.Username = "<username>";
                    apiRequest.Password = "<password>";
                    string request = JsonConvert.SerializeObject(apiRequest);
                    mpgsLogTransaction.InsertMPGSLog(model.OrderNo, model.ApiOperation, request, response);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(this.GetType()).Error("orderNo : " + model.OrderNo + " " + ex.Message, ex.InnerException);
            }

            return response;
        }

    }
}
