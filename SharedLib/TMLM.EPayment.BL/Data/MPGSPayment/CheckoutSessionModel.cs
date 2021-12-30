using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class CheckoutSessionModel
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string SuccessIndicator { get; set; }

        public static CheckoutSessionModel toCheckoutSessionModel(string response,out ResponseToMerchant responseToMerchant)
        {
            CheckoutSessionModel model = new CheckoutSessionModel();
            responseToMerchant = new ResponseToMerchant();
            try
            {
                JObject jObject = JObject.Parse(response);
                if (jObject["session"] != null)
                {
                    model = jObject["session"].ToObject<CheckoutSessionModel>();
                    model.SuccessIndicator = jObject["successIndicator"] != null ? jObject["successIndicator"].ToString() : "";
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
    }
}