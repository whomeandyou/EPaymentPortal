using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using TMLM.EPayment.BL.PaymentProvider.MPGS;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class SecureIdEnrollmentResponseModel
    {
        public string VeResEnrolled { get; set; }
        public string AcsUrl { get; set; }
        public string GatewayRecommendation { get; set; }
        public string AuthenticationStatus { get; set; }
        public string Pareq { get; set; }
        public string CReq { get; set; }
        public string MdValue { get; set; }
        public string returnURL { get; set; }
        public string termURL { get; set; }
        public string acsEci { get; set; }
        public string authenticationToken { get; set; }
        public string transactionId { get; set; }
        public string transactionStatus { get; set; }
        public string statusReasonCode { get; set; }
        public string pares { get; set; }
        public string version { get; set; }
        public string orderNumber { get; set; }
        public string CreditCardNumber { get; set; }
        public string gatewayCode { get; set; }
        public string ReceiveTransactionID { get; set; }


        public static SecureIdEnrollmentResponseModel toSecureIdEnrollmentResponseModel(string response,out ResponseToMerchant responseToMerchantOut)
        {
            SecureIdEnrollmentResponseModel model = new SecureIdEnrollmentResponseModel();
            responseToMerchantOut = new ResponseToMerchant();

            try
            {
                JObject jObject = JObject.Parse(response);

                if (jObject["error"] != null)
                {
                    responseToMerchantOut.ErrorMessage = jObject["error"]["explanation"].ToString();
                    return model;
                }
                var token3ds1 = jObject.SelectToken("authentication.3ds1.veResEnrolled");
                if (token3ds1 != null)
                {
                    model.VeResEnrolled = jObject["authentication"]["3ds1"]["veResEnrolled"].Value<string>();
                }
                model.AuthenticationStatus = jObject["order"]["authenticationStatus"].Value<string>();
                model.GatewayRecommendation = jObject["response"]["gatewayRecommendation"].Value<string>();
                return model;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public static SecureIdEnrollmentResponseModel PayerAuthenticateResponseModel(string response, out ResponseToMerchant responseToMerchantOut)
        {
            SecureIdEnrollmentResponseModel model = new SecureIdEnrollmentResponseModel();
            responseToMerchantOut = new ResponseToMerchant();

            try
            {
                JObject jObject = JObject.Parse(response);

                model.GatewayRecommendation = jObject["response"]["gatewayRecommendation"].Value<string>();
                model.version = jObject["authentication"]["version"].Value<string>();

                var token3ds2 = jObject.SelectToken("authentication.redirect.customized.3DS.acsUrl");
                var token3ds1 = jObject.SelectToken("authentication.redirectHtml");

                model.gatewayCode = jObject["response"]["gatewayCode"]?.Value<string>();

                if (token3ds2 != null)
                {
                    model.AcsUrl = jObject["authentication"]["redirect"]["customized"]["3DS"]["acsUrl"].Value<string>();
                    model.CReq = jObject["authentication"]["redirect"]["customized"]["3DS"]["cReq"].Value<string>();
                    model.transactionStatus = jObject["authentication"]["3ds2"]["transactionStatus"]?.Value<string>();
                }

                if (token3ds1 != null && model.version == "3DS1")
                {
                    model.VeResEnrolled = jObject["authentication"]["3ds1"]["veResEnrolled"].Value<string>();
                    string redirectHtml = jObject["authentication"]["redirectHtml"].Value<string>();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(redirectHtml);
                    
                    model.AcsUrl = doc.GetElementbyId("redirectTo3ds1Form").Attributes["action"].Value.ToString();
                    model.Pareq = doc.GetElementbyId("redirectTo3ds1Form").Descendants("input").Where(n => n.Attributes["name"] != null && n.Attributes["name"].Value == "PaReq").SingleOrDefault().Attributes["value"].Value.ToString();

                    var termUrl = doc.DocumentNode.SelectSingleNode("//input[@name=\"TermUrl\"]");
                    model.termURL = termUrl.GetAttributeValue("value", "");

                }

                if (jObject["error"] != null)
                {
                    responseToMerchantOut.ErrorMessage = jObject["error"]["explanation"].ToString();
                    return model;
                }

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static SecureIdEnrollmentResponseModel EnrollmentResponseModel(string response, out ResponseToMerchant responseToMerchantOut)
        {
            SecureIdEnrollmentResponseModel model = new SecureIdEnrollmentResponseModel();
            responseToMerchantOut = new ResponseToMerchant();

            try
            {
                JObject jObject = JObject.Parse(response);

                if (jObject["error"] != null)
                {
                    responseToMerchantOut.ErrorMessage = jObject["error"]["explanation"].ToString();
                    return model;
                }

                if (jObject["authentication"] != null)
                {
                    model.version = jObject["authentication"]["version"].Value<string>();
                }

                model.CreditCardNumber = jObject["sourceOfFunds"]["provided"]["card"]["number"]?.Value<string>();
                model.orderNumber = jObject["order"]["id"]?.Value<string>();

                JToken token = jObject["authentication"]["version"];
                if (token?.ToString() == "3DS2")
                {
                    model.transactionStatus = jObject["authentication"]["3ds2"]["transactionStatus"].Value<string>();
                    model.statusReasonCode = jObject["authentication"]["3ds2"]["statusReasonCode"]?.Value<string>();
                }
                else if (token?.ToString() == "3DS1")
                {
                    model.VeResEnrolled = jObject["authentication"]["3ds1"]["veResEnrolled"].Value<string>();
                    model.pares = jObject["authentication"]["3ds1"]["paResStatus"]?.Value<string>();
                }

                if (jObject["authentication"]["3ds"] != null)
                {
                    model.transactionId = jObject["authentication"]["3ds"]["transactionId"]?.Value<string>();
                    model.acsEci = jObject["authentication"]["3ds"]["acsEci"]?.Value<string>();
                    model.authenticationToken = jObject["authentication"]["3ds"]["authenticationToken"]?.Value<string>();
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