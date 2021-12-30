using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace TMLM.EPayment.BL.Data.MPGSPayment
{
    public class VoidPaymentResponseModel
    {
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }


        public static VoidPaymentResponseModel toPayVoidPaymentResponseModel(string response)
        {
            VoidPaymentResponseModel model = new VoidPaymentResponseModel();
            try
            {
                JObject jObject = JObject.Parse(response);

                if (jObject["authorizationResponse"]["responseCode"] != null)
                {
                    model.ResponseCode = jObject["response"]["acquirerCode"].Value<string>();
                    model.ErrorMessage = jObject["response"]["gatewayCode"].Value<string>();
                }

                if (jObject["error"] != null)
                {
                    model.ErrorMessage = jObject["error"]["explanation"].ToString();
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
