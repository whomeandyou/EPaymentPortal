using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;

namespace TMLM.EPayment.WebApi
{
    public class ResponseMetadata
    {
        public string Version { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public object Result { get; set; }
        public object Content { get; set; }
        public DateTime Timestamp { get; set; }
        public long? Size { get; set; }
    }

    public class TMLM_ResponseHandler : DelegatingHandler
    {
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            try
            {
                return GenerateResponse(request, response);
            }
            catch (Exception ex)
            {
                return request.CreateResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        private HttpResponseMessage GenerateResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            string errorMessage = null;
            System.Net.HttpStatusCode statusCode = response.StatusCode;
            if (!IsResponseValid(response))
            {
                return request.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Invalid response..");
            }
            object responseContent;
            if (response.TryGetContentValue(out responseContent))
            {
                System.Web.Http.HttpError httpError = responseContent as System.Web.Http.HttpError;
                if (httpError != null)
                {
                    errorMessage = httpError.Message;
                    statusCode = System.Net.HttpStatusCode.InternalServerError;
                    responseContent = null;
                }
            }
            ResponseMetadata responseMetadata = new ResponseMetadata();
            responseMetadata.Version = "1.0";
            responseMetadata.StatusCode = statusCode;
            responseMetadata.Content = responseContent;
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            responseMetadata.Timestamp = dt;
            responseMetadata.ErrorMessage = errorMessage;
            responseMetadata.Size = responseContent.ToString().Length;
            var result = request.CreateResponse(response.StatusCode, responseMetadata);
            return result;
        }
        private bool IsResponseValid(HttpResponseMessage response)
        {
            if ((response != null) && (response.StatusCode == System.Net.HttpStatusCode.OK))
                return true;
            return false;
        }
    }
}