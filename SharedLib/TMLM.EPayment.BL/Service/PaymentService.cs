using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.Common;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.BL.Gateway;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.Db.Repositories;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class PaymentService : IDisposable
    {

        bool disposed = false;

        public PaymentService() { }

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

        public OutputModel InitiatePayment(InitiatePaymentInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "InitiatePayment";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<OutputModel>(apiResult);
        }

        public GetHtmlOutputModel GetHtml(GetHtmlInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "GenerateHtml";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<GetHtmlOutputModel>(apiResult);
        }

        public GetHtmlOutputModel GetEMandateHtml(GetHtmlInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "GenerateMandateHtml";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<GetHtmlOutputModel>(apiResult);
        }

        public OutputModel CancelPayment(string transactionNumber)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "CancelTrans";
            apiRequest.Payload = JsonConvert.SerializeObject(new { transactionNumber = transactionNumber });

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<OutputModel>(apiResult);
        }

        public OutputModel FailPaymentWithStatus(string transactionNumber, string status)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "FailWithStatus";
            apiRequest.Payload = JsonConvert.SerializeObject(new { transactionNumber = transactionNumber, status = status });

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<OutputModel>(apiResult);
        }

        public ProcessPaymentOutputModel ProcessPayment(ProcessPaymentInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "ProcessPayment";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<ProcessPaymentOutputModel>(apiResult);
        }

        public void CallbackClient(InquiryPaymentInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "CallbackClient";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            apiClient.SendTransaction(apiRequest);
        }

        public void UpdatePaymentInformation(UpdatePaymentInformationInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "UpdateInfo";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            //return JsonConvert.DeserializeObject<OutputModel>(apiResult);
        }

        public OutputModel UpdateEMandateInformation(UpdatePaymentInformationInputModel model)
        {
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "UpdateEMandateInfo";
            apiRequest.Payload = JsonConvert.SerializeObject(model);

            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);
            return JsonConvert.DeserializeObject<OutputModel>(apiResult);
        }

        public InquiryPaymentOutputModel InquiryPayment(InquiryPaymentInputModel model)
        {
            
            var apiRequest = new ApiRequest();
            apiRequest.ApiMethod = ApiClient.POST;
            apiRequest.RequestUrl = PaymentSettings.BASE_URL + "InquiryPayment";
            apiRequest.Payload = JsonConvert.SerializeObject(model);
            var apiClient = new ApiClient();
            var apiResult = apiClient.SendTransaction(apiRequest);

            return JsonConvert.DeserializeObject<InquiryPaymentOutputModel>(apiResult);
        }
    }
}
