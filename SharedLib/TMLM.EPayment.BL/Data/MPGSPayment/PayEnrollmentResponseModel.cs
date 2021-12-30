using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class PayEnrollmentResponseModel
    {
        public string Status { get; set; }
        public string transactionID { get; set; }
        public string responseCode { get; set; }

        public static PayEnrollmentResponseModel toPayResponseModel(string response,out ResponseToMerchant responseToMerchant)
        {
            PayEnrollmentResponseModel model = new PayEnrollmentResponseModel();
            responseToMerchant = new ResponseToMerchant();
            try
            {
                JObject jObject = JObject.Parse(response);
                if (jObject["response"] != null)
                {
                    model.Status = jObject["response"]["acquirerCode"].Value<string>();

                    if (jObject["transaction"]["acquirer"]["transactionId"] != null)
                    {
                        model.transactionID = jObject["transaction"]["acquirer"]["transactionId"].Value<string>();
                    }
                    model.responseCode = jObject["authorizationResponse"]["responseCode"].Value<string>();

                    responseToMerchant.ResponseCode = jObject["response"]["acquirerCode"].Value<string>();
                    responseToMerchant.ErrorMessage = jObject["response"]["gatewayCode"].Value<string>();
                    responseToMerchant.ExpiryMonth = jObject["sourceOfFunds"]["provided"]["card"]["expiry"]["month"].Value<string>();
                    responseToMerchant.ExpiryYear = jObject["sourceOfFunds"]["provided"]["card"]["expiry"]["year"].Value<string>();
                    responseToMerchant.CardType = jObject["sourceOfFunds"]["provided"]["card"]["brand"].Value<string>();
                    responseToMerchant.CardMethod = jObject["sourceOfFunds"]["provided"]["card"]["fundingMethod"].Value<string>();
                    responseToMerchant.TransCode = jObject["transaction"]["reference"].Value<string>();
                    if (jObject["authentication"] != null)
                    {
                        responseToMerchant.dsVersion = jObject["authentication"]["version"].Value<string>();
                        if (jObject["authentication"]["3ds"] != null)
                        {
                            responseToMerchant.AcsEci = jObject["authentication"]["3ds"]["acsEci"].Value<string>();
                            responseToMerchant.TransactionId = jObject["authentication"]["3ds"]["transactionId"].Value<string>();
                            responseToMerchant.AuthenticationToken = jObject["authentication"]["3ds"]["authenticationToken"].Value<string>();
                        }

                        JToken token = jObject["authentication"]["version"];
                        responseToMerchant.dsVersion = jObject["authentication"]["version"].Value<string>();
                        if (token?.ToString() == "3DS2")
                        {
                            responseToMerchant.Pares = jObject["authentication"]["3ds2"]["transactionStatus"].Value<string>();
                        }
                        else if (token?.ToString() == "3DS1")
                        {
                            responseToMerchant.Veres = jObject["authentication"]["3ds1"]["veResEnrolled"].Value<string>();
                        }
                    }

                    if (jObject["sourceOfFunds"]["provided"]["card"]["issuer"] != null)
                    {
                        responseToMerchant.BankName = jObject["sourceOfFunds"]["provided"]["card"]["issuer"].Value<string>();
                    }

                    if (model.responseCode == "00")
                    {
                        responseToMerchant.AuthCode = jObject["transaction"]["authorizationCode"].Value<string>();
                    }

            

                }
                if (jObject["error"] != null)
                {
                    responseToMerchant.ErrorMessage = jObject["error"]["explanation"].ToString();
                }

                return model;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public static PayEnrollmentResponseModel toVerifyResponseModel(string response, out ResponseToMerchant responseToMerchant)
        {
            PayEnrollmentResponseModel model = new PayEnrollmentResponseModel();
            responseToMerchant = new ResponseToMerchant();
            try
            {
                JObject jObject = JObject.Parse(response);
                if (jObject["response"] != null)
                {
                    model.Status = jObject["response"]["gatewayCode"].Value<string>();

                    responseToMerchant.ResponseCode = jObject["response"]["acquirerCode"].Value<string>();
                    responseToMerchant.ErrorMessage = jObject["response"]["gatewayCode"].Value<string>();

                    if(jObject["sourceOfFunds"]["provided"]["card"]["expiry"] != null)
                    {
                        responseToMerchant.ExpiryMonth = jObject["sourceOfFunds"]["provided"]["card"]["expiry"]["month"].Value<string>();
                        responseToMerchant.ExpiryYear = jObject["sourceOfFunds"]["provided"]["card"]["expiry"]["year"].Value<string>();
                    }

                    responseToMerchant.CardType = jObject["sourceOfFunds"]["provided"]["card"]["brand"].Value<string>();
                    responseToMerchant.CardMethod = jObject["sourceOfFunds"]["provided"]["card"]["fundingMethod"].Value<string>();
                    responseToMerchant.TransCode = jObject["transaction"]["reference"].Value<string>();

                    if (jObject["sourceOfFunds"]["provided"]["card"]["issuer"] != null)
                    {
                        responseToMerchant.BankName = jObject["sourceOfFunds"]["provided"]["card"]["issuer"].Value<string>();
                    }

                    //if (jObject["authentication"]["3ds"] != null)
                    //{
                    //    responseToMerchant.AcsEci = jObject["authentication"]["3ds"]["acsEci"].Value<string>();
                    //    responseToMerchant.AuthenticationToken = jObject["authentication"]["3ds"]["authenticationToken"].Value<string>();
                    //    responseToMerchant.TransactionId = jObject["authentication"]["3ds"]["transactionId"].Value<string>();
                    //}

                    //JToken token = jObject["authentication"]["version"];
                    //responseToMerchant.dsVersion = jObject["authentication"]["version"].Value<string>();
                    //if (token?.ToString() == "3DS2")
                    //{
                    //    responseToMerchant.Pares = jObject["authentication"]["3ds2"]["transactionStatus"].Value<string>();
                    //}
                    //else if (token?.ToString() == "3DS1")
                    //{
                    //    responseToMerchant.Veres = jObject["authentication"]["3ds1"]["veResEnrolled"].Value<string>();
                    //}
                }
                if (jObject["error"] != null)
                {
                    responseToMerchant.ErrorMessage = jObject["error"]["explanation"].ToString();
                }

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}