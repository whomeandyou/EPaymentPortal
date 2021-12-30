using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Http;
using TMLM.EPayment.BL.Gateway;
using TMLM.EPayment.BL.PaymentProvider.MPGS;
using TMLM.EPayment.WebApi;
using TMLM.EPayment.WebApi.Gateway;

namespace TMLM.EForm.Web.Controllers
{
    public class MPGSPaymentApiController : ApiController
    {
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

            return response;
        }


        public string UpdateSession(GatewayApiRequest gatewayApiRequest)
        {
            gatewayApiRequest.ApiOperation = string.Empty;
            gatewayApiRequest.buildSessionRequestUrl();
            gatewayApiRequest.SessionId = string.Empty;
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

            return response;
        }


        public string Check3dsEnrollment(GatewayApiRequest gatewayApiRequest)
        {
            // Retrieve session
            gatewayApiRequest.buildSecureIdRequestUrl();
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

            return response;

        }

        public string ProcessACSResult(GatewayApiRequest gatewayApiRequest, string paRes)
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

            return response;

        }

    }
}
