using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Abstract;
using TMLM.EPayment.Batch.Helpers;
using TMLM.EPayment.Batch.Model;
using TMLM.EPayment.Batch.Service;

namespace TMLM.EPayment.Batch.Application
{
    public class MPGSRecurring : BatchApplication, IMPGS
    {
        private readonly string ApiHeader = "apiOperation,authentication.3ds.transactionId,authentication.3ds.acsEci,authentication.3ds.authenticationToken,authentication.3ds2.transactionStatus,authentication.3ds1.paResStatus,authentication.3ds1.veResEnrolledorder.id,transaction.id,order.reference,transaction.reference,order.amount,order.currency,sourceOfFunds.type,sourceOfFunds.provided.card.number,sourceOfFunds.provided.card.expiry.month,sourceOfFunds.provided.card.expiry.year,result,error.cause,error.explanation,response.gatewayCode,authorizationResponse.responseCode,transaction.authorizationCode";
        private readonly string FirstTimeRequestHeader = "apiOperation,authentication.3ds.transactionId,authentication.3ds.acsEci,authentication.3ds.authenticationToken,authentication.3ds2.transactionStatus,authentication.3ds1.veResEnrolled,order.id,transaction.id,order.reference,transaction.reference,order.amount,order.currency,order.statementDescriptor.name,sourceOfFunds.type,sourceOfFunds.provided.card.number,sourceOfFunds.provided.card.expiry.month,sourceOfFunds.provided.card.expiry.year,transaction.source,sourceOfFunds.provided.card.storedOnFile,agreement.id,agreement.type,result,error.cause,error.explanation,response.gatewayCode,authorizationResponse.responseCode,transaction.authorizationCode,sourceOfFunds.provided.card.brand,sourceOfFunds.provided.card.fundingMethod";
        private readonly string RecurringRequestHeader = "apiOperation,order.id,transaction.id,order.reference,transaction.reference,order.amount,order.currency,order.statementDescriptor.name,sourceOfFunds.type,sourceOfFunds.provided.card.number,sourceOfFunds.provided.card.expiry.month,sourceOfFunds.provided.card.expiry.year,transaction.source,sourceOfFunds.provided.card.storedOnFile,agreement.id,result,error.cause,error.explanation,response.gatewayCode,authorizationResponse.responseCode,transaction.authorizationCode,sourceOfFunds.provided.card.brand,sourceOfFunds.provided.card.fundingMethod";
        private List<MPGSResponseModel> ListOfResponse = new List<MPGSResponseModel>();
        public readonly static string Report = ConfigurationHelper.GetValue("file.report.path");

        #region New Implementation 
        public MPGSRecurring(Batch.Data.BatchPaymentRepo repo, Batch.Data.EPaymentRepo repoEpayment) : base(repo,repoEpayment) { }

        public override BatchProcess DataFileExtration(string fullFilePath)
        {
            return base.DataFileExtration(fullFilePath);
        }

        public async Task<bool> RunFirstTimeBatchBilling(BatchProcess batchData, List<ExtractDataModel> data)
        {
            if (data == null || data.Count <= 0) return true;

            LogHelper.Info("Initial Configure the Rest API");

            var counter = 0;
            string uploadBatch = string.Empty;
            string dataContent = string.Empty;
            string result = string.Empty;
            var isServiceOnline = false;

            var dataUniqueId = Guid.NewGuid().ToString().Replace("-", "");
            if (batchData.ProcessingStatus == "Failed" && batchData.StatusId > (int)Data.BatchStatus.MPGSServiceFileUpload)
            {
                dataUniqueId = batchData.UrlGuId;
                uploadBatch = batchData.BatchData;
                dataContent = batchData.DataContent;
            }
            var baseUrl = ConfigurationHelper.GetValue("mpgs.baseUrl");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + ConfigurationHelper.GetValue("mpgs.merchant.password")));
            var encodingType = ConfigurationHelper.GetValue("mpgs.encodingType");
            var version = ConfigurationHelper.GetValue("mpgs.api.version");
            var merchantId = ConfigurationHelper.GetValue("mpgs.merchant.id");
            var information = ConfigurationHelper.GetValue("mpgs.url-information").Replace("{version}", version);
            var batch = ConfigurationHelper.GetValue("mpgs.url-batch").Replace("{version}", version).Replace("{merchantid}", merchantId).Replace("{uniqueid}", dataUniqueId);
            var response = ConfigurationHelper.GetValue("mpgs.url-response");
            var status = ConfigurationHelper.GetValue("mpgs.url-status");
            var validate = ConfigurationHelper.GetValue("mpgs.url-validation");
            var contentType = ConfigurationHelper.GetValue("mpgs.ContentType");
            int errorRetry = int.Parse(ConfigurationHelper.GetValue("ErrorRetry"));

            try
            {
                _repo.UpdateBatchPaymentUrl(batchData.BatchID, dataUniqueId, false);

                _repo.UpdateBatchProcessStatus(batchData, 
                                                new PaymentRequestModel() 
                                                { 
                                                    BatchId = batchData.BatchID 
                                                }, 
                                                Data.BatchStatus.DataInitilization, 
                                                $"Url: {baseUrl}, dataUniqueId: {dataUniqueId}, bath: {batch}");

                using (var apiHelper = new RestApiHelper<MPGSResponseModel>(baseUrl, password, encodingType, contentType))
                {
                    LogHelper.Info("Start Rest API Process for MPGS");
                    _repo.UpdateBatchProcessStatus(batchData, 
                                                    new PaymentRequestModel() 
                                                    { 
                                                        BatchId = batchData.BatchID, 
                                                        RequestUrl = $"{baseUrl}{information}"
                                                    },
                                                    Data.BatchStatus.MPGSServicePing,
                                                    $"Url: {baseUrl}{information}");

                    //var isServiceOnline = await apiHelper.PingServerAsync(information);
                    var serviceResult = await apiHelper.CallRestApiString(information, HttpMethod.Get, "", encodingType);
                    if (serviceResult != null && serviceResult.ToString().ToUpper().Contains("OPERATING"))
                        isServiceOnline = true;

                    if (isServiceOnline)
                    {
                        LogHelper.Info("The MPGS services is Alive. Initiate the batch process");

                        #region 1. Uploading data to MPGS
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceFileUpload) || batchData.ProcessingStatus == "Processing")
                        {
                            dataContent = this.FirstTimeRequestData(data);
                            _repo.UpdateBatchProcessStatus(batchData,
                                                            new PaymentRequestModel()
                                                            {
                                                                BatchId = batchData.BatchID,
                                                                RequestUrl = $"{baseUrl}{batch}",
                                                                RequestBody = EncrpytionDescrpytion.EncryptString(dataContent)
                                                            },
                                                            Data.BatchStatus.MPGSServicePing, "");
                            do
                            {
                                try
                                {
                                    uploadBatch = await apiHelper.CallRestApiString(batch, HttpMethod.Put, dataContent, encodingType);
                                    if (uploadBatch.ToUpper().Contains(ApiStatus.Fail))
                                    {
                                        LogHelper.Info("File Fails to Upload the MPGS API");
                                        _repo.UpdateBatchProcessStatus(batchData,
                                                                        new PaymentRequestModel()
                                                                        {
                                                                            BatchId = batchData.BatchID,
                                                                            Status = "Failed to upload",
                                                                            RequestUrl = $"{baseUrl}{batch}",
                                                                            RequestBody = EncrpytionDescrpytion.EncryptString(dataContent)
                                                                        },
                                                                        Data.BatchStatus.MPGSServiceFileUpload, "");
                                        throw new Exception("File Fails to Upload the MPGS API");
                                    }
                                    else
                                    {
                                        LogHelper.Info("Successfully upload the batch.");
                                        _repo.UpdateBatchProcessStatus(batchData,
                                                                        new PaymentRequestModel()
                                                                        {
                                                                            BatchId = batchData.BatchID,
                                                                            Status = "Uploaded Successfully",
                                                                            RequestUrl = $"{baseUrl}{batch}",
                                                                            RequestBody = EncrpytionDescrpytion.EncryptString(dataContent)
                                                                        },
                                                                        Data.BatchStatus.MPGSServiceFileUpload, "");
                                        _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceFileUpload);
                                        _repo.UpdateBatchData(batchData.BatchID, dataContent, uploadBatch);
                                        batchData.ProcessingStatus = "Processing";
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Upload data to MPGS Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Upload data to MPGS Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to upload after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceFileUpload, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceFileUpload);
                                return false;
                            }
                        }
                        #endregion

                        #region 2. Validate uploaded data status
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceValidateUploadedRequest) || batchData.ProcessingStatus == "Processing")
                        {
                            do
                            {
                                try
                                {
                                    
                                    while (!uploadBatch.Contains(ApiStatus.Upload))
                                    {
                                        uploadBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Failed to validate Uploaded content", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = uploadBatch }, Data.BatchStatus.MPGSServiceValidateUploadedRequest, "");
                                    }
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceFileUpload);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Validate uploaded data status Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Validate uploaded data status Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed Validate uploaded data status {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceValidateUploadedRequest, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceValidateUploadedRequest);
                                return false;
                            }
                        }
                        #endregion

                        #region 3. Validate the batch
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceBatchValidation) || batchData.ProcessingStatus == "Processing")
                        {
                            do
                            {
                                try
                                {
                                    LogHelper.Info("Validate the batch");
                                    var HashContent = EncrpytionDescrpytion.EncryptString(dataContent);
                                    var validateBatch = await apiHelper.CallRestApiString($"{batch}{validate}", HttpMethod.Post, HashContent, encodingType);
                                    LogHelper.Info("Validate Success. MPGS start process the batch.");
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Validated the batch and process", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, validate), RequestBody = "", ResponseBody = validateBatch }
                                                                    , Data.BatchStatus.MPGSServiceBatchValidation, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceBatchValidation);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Validate the batch Unexpected Error: ", ex);
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Validate the batch Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to validate batch after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceBatchValidation, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceBatchValidation);
                                return false;
                            }
                        }
                        #endregion

                        #region 4. Batch in process
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceRetriveProcessedResult) || batchData.ProcessingStatus == "Processing")
                        {
                            do
                            {
                                try
                                {
                                    var statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                    while (!statusBatch.Contains(ApiStatus.Complete))
                                    {
                                        LogHelper.Info("MPGS Batch Service currently still processing the information...");
                                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Data is processing (WIP)", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = statusBatch }
                                                                    , Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                        
                                        statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                        await Task.Delay((5000));
                                    }
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Data being processed", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = statusBatch }
                                                                    , Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Data Processing (Recurring) Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Data Processing (Recurring) Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed Process Data (Recurring) after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                return false;
                            }
                        }
                        #endregion

                        #region 5. Retrive result 
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceRetriveProcessedResult) || batchData.ProcessingStatus == "Processing")
                        {
                            LogHelper.Info("Fetching the batch information.");
                            do
                            {
                                try
                                {
                                    result = await apiHelper.CallRestApiString($"{batch}{response}", HttpMethod.Get, "", encodingType);
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Completed", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, response), RequestBody = "", ResponseBody = result }
                                                                    , Data.BatchStatus.Completed, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Retrive result Unexpected Error: ", ex);
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Retrive result Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to retrive result after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                return false;
                            }
                        }
                        #endregion

                        LogHelper.Info("Store the batch information result. \n \n" + result);
                        FirstTimeResponePayload(result, batchData);

                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Store", RequestUrl = "", RequestBody = "", ResponseBody = result }
                                                        , Data.BatchStatus.Completed, String.Format("Updated back to transaction table"));
                        _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Completed", Data.BatchStatus.Completed);
                        return true;
                    }
                    else
                        return false;
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat("Start Rest API Process for MPGS", ex);
                return false;
            }
        }

        public async Task<bool> RunRecurringBatchBilling(BatchProcess batchData, List<ExtractDataModel> data)
        {
            if (data == null || data.Count <= 0) return true;

            LogHelper.Info("Initial Configure the Rest API");

            var counter = 0;
            string dataContent = string.Empty;
            string uploadBatch = string.Empty;
            string result = string.Empty;

            var dataUniqueId = Guid.NewGuid().ToString().Replace("-", "");
            if (batchData.ProcessingStatus == "Failed" && batchData.StatusId > (int)Data.BatchStatus.MPGSServiceFileUpload)
            {
                dataUniqueId = batchData.UrlGuId;
                uploadBatch = batchData.BatchData;
                dataContent = batchData.DataContent;
            }
            var baseUrl = ConfigurationHelper.GetValue("mpgs.baseUrl");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + ConfigurationHelper.GetValue("mpgs.merchant.password")));
            var encodingType = ConfigurationHelper.GetValue("mpgs.encodingType");
            var version = ConfigurationHelper.GetValue("mpgs.api.version");
            var merchantId = ConfigurationHelper.GetValue("mpgs.merchant.id");
            var information = ConfigurationHelper.GetValue("mpgs.url-information").Replace("{version}", version);
            var batch = ConfigurationHelper.GetValue("mpgs.url-batch").Replace("{version}", version).Replace("{merchantid}", merchantId).Replace("{uniqueid}", dataUniqueId);
            var response = ConfigurationHelper.GetValue("mpgs.url-response");
            var status = ConfigurationHelper.GetValue("mpgs.url-status");
            var validate = ConfigurationHelper.GetValue("mpgs.url-validation");
            var contentType = ConfigurationHelper.GetValue("mpgs.ContentType");
            int errorRetry = int.Parse(ConfigurationHelper.GetValue("ErrorRetry"));

            try
            {
                _repo.UpdateBatchPaymentUrl(batchData.BatchID, dataUniqueId, true);

                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID }, Data.BatchStatus.DataInitilization, String.Format("Url: {0}, dataUniqueId: {1}, bath: {2}", baseUrl, dataUniqueId, batch));
                using (var apiHelper = new RestApiHelper<MPGSResponseModel>(baseUrl, password, encodingType, contentType))
                {
                    LogHelper.Info("Start Rest API Process for MPGS (Recurring)");
                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, RequestUrl = String.Format("{0}{1}", baseUrl, information) }, Data.BatchStatus.MPGSServicePing, String.Format("Url: {0}{1}", baseUrl, information));

                    var isServiceOnline = await apiHelper.PingServerAsync(information);
                    if (isServiceOnline)
                    {
                        LogHelper.Info("The MPGS services is Alive. Initiate the batch process (Recurring)");

                        #region 1. Uploading data to MPGS
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceFileUpload) || batchData.ProcessingStatus == "Processing")
                        {
                            dataContent = this.RecurringRequestData(data);
                            _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServicePing, "");

                            do
                            {
                                try
                                {
                                    uploadBatch = await apiHelper.CallRestApiString(batch, HttpMethod.Put, dataContent, encodingType);
                                    if (uploadBatch.ToUpper().Contains(ApiStatus.Fail))
                                    {
                                        LogHelper.Info("File Fails to Upload the MPGS API (Recurring)");
                                        _repo.UpdateBatchProcessStatus(batchData,
                                                                        new PaymentRequestModel()
                                                                        {
                                                                            BatchId = batchData.BatchID,
                                                                            Status = "Failed to upload (Recurring)",
                                                                            RequestUrl = $"{baseUrl}{batch}",
                                                                            RequestBody = EncrpytionDescrpytion.EncryptString(dataContent)
                                                                        },
                                                                        Data.BatchStatus.MPGSServiceFileUpload, "");
                                        throw new Exception("File Fails to Upload the MPGS API (Recurring)");
                                    }
                                    else
                                    {
                                        LogHelper.Info("Successfully upload the batch.");
                                        _repo.UpdateBatchProcessStatus(batchData,
                                                                        new PaymentRequestModel()
                                                                        {
                                                                            BatchId = batchData.BatchID,
                                                                            Status = "Uploaded Successfully",
                                                                            RequestUrl = $"{baseUrl}{batch}",
                                                                            RequestBody = EncrpytionDescrpytion.EncryptString(dataContent)
                                                                        },
                                                                        Data.BatchStatus.MPGSServiceFileUpload, "");
                                        _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceFileUpload);
                                        batchData.ProcessingStatus = "Processing";
                                        _repo.UpdateBatchData(batchData.BatchID, dataContent, uploadBatch);
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Upload data to MPGS Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Upload data to MPGS Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to upload after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceFileUpload, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceFileUpload);
                                return false;
                            }
                        }
                        #endregion

                        #region 2. Validate uploaded data status
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceValidateUploadedRequest) || batchData.ProcessingStatus == "Processing")
                        {
                            do
                            {
                                try
                                {
                                    while (!uploadBatch.Contains(ApiStatus.Upload))
                                    {
                                        uploadBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Failed to validate Uploaded content (Recurring)", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = uploadBatch }, Data.BatchStatus.MPGSServiceValidateUploadedRequest, "");
                                    }
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceFileUpload);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Validate uploaded data status Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Validate uploaded data status Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed Validate uploaded data status {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceValidateUploadedRequest, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceValidateUploadedRequest);
                                return false;
                            }
                        }
                        #endregion

                        #region 3. Validate the batch 
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceBatchValidation) || batchData.ProcessingStatus == "Processing")
                        {
                        
                            LogHelper.Info("Validate the batch");
                            var HashContent = EncrpytionDescrpytion.EncryptString(dataContent);
                            do
                            {
                                try
                                {
                                    var validateBatch = await apiHelper.CallRestApiString($"{batch}{validate}", HttpMethod.Post, HashContent, encodingType);
                                    LogHelper.Info("Validate Success. MPGS start process the batch.");
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Validated the batch and process (Recurring)", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, validate), RequestBody = "", ResponseBody = validateBatch }
                                                                    , Data.BatchStatus.MPGSServiceBatchValidation, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceBatchValidation);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Validate the batch Unexpected Error: ", ex);
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Validate the batch Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to validate batch after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceBatchValidation, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceBatchValidation);
                                return false;
                            }
                        }
                        #endregion

                        #region 4. Batch in process
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceRetriveProcessedResult) || batchData.ProcessingStatus == "Processing")
                        {
                            do
                            {
                                try
                                {
                                    var statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                    while (!statusBatch.Contains(ApiStatus.Complete))
                                    {
                                        LogHelper.Info("MPGS Batch Service currently still processing the information... (Recurring)");
                                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Data is processing (WIP) (Recurring)", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = statusBatch }
                                                                    , Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");

                                        statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                                        await Task.Delay((5000));
                                    }
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Data being processed", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, status), RequestBody = "", ResponseBody = statusBatch }
                                                                    , Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Processing", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Data Processing (Recurring) Unexpected Error: ", ex.ToString());
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Data Processing (Recurring) Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed Process Data (Recurring) after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.MPGSServiceRetriveProcessedResult, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.MPGSServiceRetriveProcessedResult);
                                return false;
                            }
                        }
                        #endregion

                        #region 5. Retrive result 
                        counter = 0;
                        if ((batchData.ProcessingStatus == "Failed" && batchData.StatusId == (int)Data.BatchStatus.MPGSServiceRetriveProcessedResult) || batchData.ProcessingStatus == "Processing")
                        {
                            LogHelper.Info("Fetching the batch information. (Recurring)");
                            do
                            {
                                try
                                {
                                    result = await apiHelper.CallRestApiString($"{batch}{response}", HttpMethod.Get, "", encodingType);
                                    _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Completed (Recurring)", RequestUrl = String.Format("{0}{1}{2}", baseUrl, batch, response), RequestBody = "", ResponseBody = result }
                                                                    , Data.BatchStatus.Completed, "");
                                    _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Completed", Data.BatchStatus.Completed);
                                    batchData.ProcessingStatus = "Processing";
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.ErrorFormat("Retrive result Unexpected Error: ", ex);
                                    counter++;
                                }
                            } while (counter < errorRetry);

                            if (counter == errorRetry)
                            {
                                LogHelper.Error($"Retrive result Stopped After Trying {errorRetry} Time");
                                _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = $"Failed to retrive result after {errorRetry} time", RequestUrl = String.Format("{0}{1}", baseUrl, batch), RequestBody = EncrpytionDescrpytion.EncryptString(dataContent) }, Data.BatchStatus.Completed, "");
                                _repo.UpdateBatchProcessingStatus(batchData.BatchID, "Failed", Data.BatchStatus.Completed);
                                return false;
                            }
                        }
                        #endregion

                        LogHelper.Info("Store the batch information result. (Recurring) \n \n" + result);
                        RecurringResponePayload(result, batchData);

                        _repo.UpdateBatchProcessStatus(batchData, new PaymentRequestModel() { BatchId = batchData.BatchID, Status = "Store (Recurring)", RequestUrl = "", RequestBody = "", ResponseBody = result }
                                                        , Data.BatchStatus.Completed, "Updated back to transaction table");
                        return true;
                    }
                    else
                        return false;
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat("Start Rest API Process for MPGS", ex);
                return false;
            }
        }

        private string FirstTimeRequestData(List<ExtractDataModel> content)
        {
            string requestContent = FirstTimeRequestHeader;
            foreach (ExtractDataModel data in content)
            {
                requestContent += $"\n{data.ApiOperation},{data.dsTransactionId},{data.dsAcsEci},{data.dsAuthenticationToken},{data.Pares},{data.Veres},{data.OrderId},{data.OrderId},{data.OrderId},{data.OrderId},{data.Amount},{data.Currency},TOKIO MARINE LIFE ECOM {data.PolicyNumber},{ data.PaymentType},{data.AccountNumber},{data.ExpiryMonth},{data.ExpiryYear},INTERNET,TO_BE_STORED,{data.AgreementId},RECURRING,,,,,,,,";
            }
            return requestContent;
        }

        private void FirstTimeResponePayload(string content, BatchProcess batch)
        {
            var arrString = content.Split('\n');
            for (var i = 0; i < arrString.Length; i++)
            {
                if (!arrString[i].ToLower().Contains("apioperation"))
                {

                    FirstTimeResponseModel(arrString[i]);
                }
            }
            this._repo.UpdateTransactionDetails(ListOfResponse);
        }

        public void ExtractExistingDataResponse(List<ExtractDataModel> extractDataModel)
        {
            foreach (ExtractDataModel model in extractDataModel)
            {
                if (!string.IsNullOrEmpty(model.ResponseData))
                {
                    string content = model.ResponseData;

                    var arrResult = content.Replace("\"", "").Split(',');
                    if (arrResult.Count() > 26)
                    {
                        FirstTimeResponseModel(content);
                    }
                    else
                    {
                        RecurringResponseModel(content);
                    }
                }
            }
        }

        public void FirstTimeResponseModel(string content)
        {
            var arrResult = content.Replace("\"", "").Split(',');
            MPGSResponseModel model = new MPGSResponseModel()
            {
                ApiOperation = arrResult[0],
                dsTransactionId = arrResult[1],
                dsAcsEci = arrResult[2],
                dsAuthenticationToken = arrResult[3],
                ds2TransactionStatus = arrResult[4],
                Veres = arrResult[5],
                OrderId = arrResult[6],
                TransactionId = arrResult[7],
                OrderRef = arrResult[8],
                TransactionRef = arrResult[9],
                Amount = arrResult[10],
                Currency = arrResult[11],
                Type = arrResult[13],
                BankNumber = arrResult[14],
                ExpiryMonth = arrResult[15],
                ExpiryYear = arrResult[16],
                Result = arrResult[21],
                Error = arrResult[22],
                ErrorDescription = arrResult[22] + " " + arrResult[23],
                GatewayDesc = arrResult[24],
                GatewayCode = arrResult[25],
                AuthorizationCode = arrResult[26],
                CardBrand = arrResult[27],
                CardMethod = arrResult[28],
                Source = content
            };

            ListOfResponse.Add(model);
        }

        public void RecurringResponseModel(string content)
        {

            var arrResult = content.Replace("\"", "").Split(',');
            MPGSResponseModel model = new MPGSResponseModel()
            {
                ApiOperation = arrResult[0],
                OrderId = arrResult[1],
                TransactionId = arrResult[2],
                OrderRef = arrResult[3],
                TransactionRef = arrResult[4],
                Amount = arrResult[5],
                Currency = arrResult[6],
                Type = arrResult[8],
                BankNumber = arrResult[9],
                ExpiryMonth = arrResult[10],
                ExpiryYear = arrResult[11],
                Result = arrResult[15],
                Error = arrResult[16],
                ErrorDescription = arrResult[16] + " " + arrResult[17],
                GatewayDesc = arrResult[18],
                GatewayCode = arrResult[19],
                AuthorizationCode = arrResult[20],
                CardBrand = arrResult[21],
                CardMethod = arrResult[22],
                Source = content
            };
      
            ListOfResponse.Add(model);
        }

        private string RecurringRequestData(List<ExtractDataModel> content)
        {
            string requestContent = RecurringRequestHeader;
            foreach (ExtractDataModel data in content)
            {
                requestContent += $"\n{data.ApiOperation},{data.OrderId},{data.OrderId},{data.OrderId},{data.OrderId},{data.Amount},{data.Currency},TOKIO MARINE LIFE ECOM {data.PolicyNumber},{data.PaymentType},{data.AccountNumber},{data.ExpiryMonth},{data.ExpiryYear},MERCHANT,STORED,{data.AgreementId},,,,,,,,";
            }
            return requestContent;
        }

        private void RecurringResponePayload(string content, BatchProcess batch)
        {
     
            var arrString = content.Split('\n');

            for (var i = 0; i < arrString.Length; i++)
            {
                if (!arrString[i].ToLower().Contains("apioperation"))
                {
                    RecurringResponseModel(arrString[i]);
                }
            }

            this._repo.UpdateTransactionDetails(ListOfResponse);
        }
        #endregion

        public void GenerateFlatFile(BatchProcess batchData)
        {
            try
            {
                if (_repo.CheckMPGSStatus(batchData.BatchID))
                {
                    EmailServices.SendEmailWithOutAttachment("Batch Payment Process Error Notification", "Batch Payment Process Error.\nBatch ID : " + batchData.BatchID, ConfigurationHelper.GetValue("ToEmailError"));
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(String.Format("Check MPGS Status error : ", ex));
                return;
            }

            var outputSuccessResult = base.Header;// Header.Insert(5,"0");
            var outputFailResult = base.Header;

            //change to 000000
            //var RefNumber = 200340010000;
            var RefNumber = 1;
            int successRecordCount = 0;
            int failRecordCount = 0;

            decimal successTotalAmount = 0;
            decimal failTotalAmount = 0;
            string outputFileName = FileServices.GetFileNameWithoutExt(batchData.FileName).Remove(7, 1).Insert(7, "R");
            string outputPath = $"{Output}{outputFileName}";

            foreach (ExtractDataModel data in batchData.ExtractedData)
            {
                MPGSResponseModel responseData = ListOfResponse.Find(p => p.OrderId == data.OrderId);
                if (responseData == null) responseData = new MPGSResponseModel() { AuthorizationCode = data.AuthorizationCode, GatewayCode = data.GatewayCode, Amount = data.Amount };

                if (String.IsNullOrEmpty(responseData.GatewayCode))
                {
                    responseData.GatewayCode = "14";
                }

                string APRCODE = string.Empty;

                if (!string.IsNullOrEmpty(responseData.AuthorizationCode))
                {
                    APRCODE = responseData.AuthorizationCode; //6
                }
                else
                {
                    APRCODE = "".PadLeft(6);
                }

                var RESCODE = responseData.GatewayCode; //2
                var RESCODE2 = RefNumber.ToString().PadLeft(12, '0'); //12
                var BATCHNUM = "0" + base.Header.Substring(5, 5); //6

                RefNumber++;

                string Amount = string.Empty;
                if (!string.IsNullOrEmpty(responseData.Amount))
                {
                    Amount = responseData.Amount.Replace(".", "").PadLeft(11, '0');
                }
                else
                {
                    Amount = data.Amount.Replace(".", "").PadLeft(11, '0');
                }

                if (responseData.GatewayCode.Equals("00"))
                {
                    outputSuccessResult += $"{Environment.NewLine}{data.MerchantId,-4}{data.RecordType}{data.MerchantNumber}{data.PolicyNumber.PadRight(25)}{data.AccountNumber}{data.ExpiryDate}{Amount}{data.AccountName,-26}{data.Address,-60}{data.PhoneNumber,-10}{data.PostCode}{data.SumAssured}{APRCODE}{RESCODE}{RESCODE2}{BATCHNUM}";
                    successRecordCount++;
                    successTotalAmount += Convert.ToDecimal(data.Amount);
                }
                else
                {
                    outputFailResult += $"{Environment.NewLine}{data.MerchantId,-4}{data.RecordType}{data.MerchantNumber}{data.PolicyNumber.PadRight(25)}{data.AccountNumber}{data.ExpiryDate}{Amount}{data.AccountName,-26}{data.Address,-60}{data.PhoneNumber,-10}{data.PostCode}{data.SumAssured}{APRCODE}{RESCODE}{RESCODE2}";
                    failRecordCount++;
                    failTotalAmount += Convert.ToDecimal(data.Amount);
                }
            }

            var footer = base.Footer.Substring(0, 18);
            var strSuccessAmount = String.Format("{0:0000000000000}", Convert.ToInt32((successTotalAmount * 100)));
            var strFailAmount = String.Format("{0:0000000000000}", Convert.ToInt32((failTotalAmount * 100)));
            var strSuccessRecordCount = String.Format("{0:00000}", successRecordCount);
            var strFailRecordCount = String.Format("{0:00000}", failRecordCount);

            var SuccessFooter = $"{footer}{strSuccessRecordCount}{strSuccessAmount}";
            var FailFooter = $"{footer}{strFailRecordCount}{strFailAmount}";

            outputSuccessResult += $"{Environment.NewLine}{SuccessFooter}";
            outputFailResult += $"{Environment.NewLine}{FailFooter}";

            FileServices.WriteToFile(outputPath + "A1.txt", outputSuccessResult);
            FileServices.WriteToFile(outputPath + "D1.txt", outputFailResult);

            var processPath = $"{Processing}/{FileName}";
            var competePath = $"{Completed}/{FileName}";
            FileServices.MoveFile(processPath, competePath);

            try
            {
                EmailServices.SendEmail("Batch Process Complete Notification", $"{FileName} has been process complete. \n Total Data in File Count : {batchData.ExtractedData.Count} || Total Return Count : {batchData.ExtractedData.FindAll(p => !String.IsNullOrEmpty(p.GatewayCode)).Count} \n Number of success record : {successRecordCount} || Number of failed record : {failRecordCount}", ConfigurationHelper.GetValue("ToEmail"), outputPath + "A1.txt", outputPath + "D1.txt");
            }
            catch (Exception ex)
            {
                LogHelper.Error(String.Format("Failed to send email", ex));
            }
        }

        //public void GenerateReconsolidationReport(BatchProcess batchData)
        //{
        //    try
        //    {
        //        if (_repo.CheckMPGSStatus(batchData.BatchID))
        //        {
        //            EmailServices.SendEmailWithOutAttachment("Batch Payment Process Error Notification", "Batch Payment Process Error.\nBatch ID : " + batchData.BatchID, ConfigurationHelper.GetValue("ToEmailError"));
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(String.Format("Check MPGS Status error : ", ex));
        //        return;
        //    }

        //    ExcelPackage excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Detail");
        //    var workSheet2 = excel.Workbook.Worksheets.Add("Summary");
        //    var workSheet3 = excel.Workbook.Worksheets.Add("Compare");

        //    // setting the properties
        //    // of the work sheet 
        //    workSheet.TabColor = System.Drawing.Color.Black;
        //    workSheet.DefaultRowHeight = 12;

        //    // Setting the properties
        //    // of the first row
        //    workSheet.Row(1).Height = 20;
        //    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet.Row(1).Style.Font.Bold = true;
        //    workSheet2.Row(1).Height = 20;
        //    workSheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet2.Row(1).Style.Font.Bold = true;
        //    workSheet3.Row(1).Height = 20;
        //    workSheet3.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet3.Row(1).Style.Font.Bold = true;

        //    // Header of the Excel sheet
        //    workSheet.Cells[1, 1].Value = "MerchantName";
        //    workSheet.Cells[1, 2].Value = "PolicyNumber";
        //    workSheet.Cells[1, 3].Value = "Amount";
        //    workSheet.Cells[1, 4].Value = "CardBrand";
        //    workSheet.Cells[1, 5].Value = "CardMethod";
        //    workSheet.Cells[1, 6].Value = "Error";
        //    workSheet.Cells[1, 7].Value = "ErrorExplanation";

        //    workSheet2.Cells[1, 1].Value = "CardBrand";
        //    workSheet2.Cells[1, 2].Value = "CardMethod";
        //    workSheet2.Cells[1, 3].Value = "GatewayCode";
        //    workSheet2.Cells[1, 4].Value = "TotalFail";
        //    workSheet2.Cells[1, 5].Value = "TotalFailAmount";
        //    workSheet2.Cells[1, 6].Value = "TotalInvalidRequestFailAmount";
        //    workSheet2.Cells[1, 8].Value = "CardBrand";
        //    workSheet2.Cells[1, 9].Value = "CardMethod";
        //    workSheet2.Cells[1, 10].Value = "GatewayCode";
        //    workSheet2.Cells[1, 11].Value = "TotalSuccess";
        //    workSheet2.Cells[1, 12].Value = "TotalSuccessAmount";

        //    workSheet3.Cells[1, 1].Value = "TextFileAmount";
        //    workSheet3.Cells[1, 2].Value = "MpgsAmount";
        //    workSheet3.Cells[1, 3].Value = "Mpgs/Text Amount Difference";

        //    int recordIndex = 2, SummaryrecordIndex = 2, SummaryrecordIndex2 = 2;
        //    int invalidRequestCount = 0;
        //    decimal successTotalAmount = 0;

        //    try
        //    {
        //        List<SummaryInfo> summary = new List<SummaryInfo>();
        //        summary = _repo.GetSummaryResult(batchData.BatchID);

        //        foreach (var grp in summary)
        //        {
        //            if (grp.GatewayCode == "00")
        //            {
        //                workSheet2.Cells[SummaryrecordIndex, 8].Value = grp.CardType;
        //                workSheet2.Cells[SummaryrecordIndex, 9].Value = grp.CardMethod;
        //                workSheet2.Cells[SummaryrecordIndex, 10].Value = grp.GatewayCode;
        //                workSheet2.Cells[SummaryrecordIndex, 11].Value = grp.Total;
        //                workSheet2.Cells[SummaryrecordIndex, 12].Value = grp.Amount;
        //                SummaryrecordIndex++;
        //            }
        //            else
        //            {
        //                workSheet2.Cells[SummaryrecordIndex2, 1].Value = grp.CardType;
        //                workSheet2.Cells[SummaryrecordIndex2, 2].Value = grp.CardMethod;
        //                workSheet2.Cells[SummaryrecordIndex2, 3].Value = grp.GatewayCode;
        //                workSheet2.Cells[SummaryrecordIndex2, 4].Value = grp.Total;
        //                workSheet2.Cells[SummaryrecordIndex2, 5].Value = grp.Amount;
        //                SummaryrecordIndex2++;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    };

        //    foreach (ExtractDataModel data in batchData.ExtractedData)
        //    {
        //        MPGSResponseModel responseData = ListOfResponse.Find(p => p.OrderId == data.OrderId);
        //        batchData.TextFileAmount = batchData.TextFileAmount + decimal.Parse(data.Amount);
        //        if (responseData == null) responseData = new MPGSResponseModel() { AuthorizationCode = data.AuthorizationCode, GatewayCode = data.GatewayCode, Amount = data.Amount };

        //        if (String.IsNullOrEmpty(responseData.GatewayCode))
        //        {
        //            responseData.GatewayCode = "14";
        //        }

        //        if (responseData.GatewayCode.Equals("00"))
        //        {
        //            workSheet.Cells[recordIndex, 1].Value = data.MerchantNumber;
        //            workSheet.Cells[recordIndex, 2].Value = data.PolicyNumber;
        //            workSheet.Cells[recordIndex, 3].Value = data.Amount;
        //            workSheet.Cells[recordIndex, 4].Value = data.CardBrand;
        //            workSheet.Cells[recordIndex, 5].Value = data.CardMethod;

        //            recordIndex++;
        //            successTotalAmount += Convert.ToDecimal(data.Amount);
        //        }
        //        else
        //        {
        //            workSheet.Cells[recordIndex, 1].Value = data.MerchantNumber;
        //            workSheet.Cells[recordIndex, 2].Value = data.PolicyNumber;
        //            workSheet.Cells[recordIndex, 3].Value = data.Amount;
        //            workSheet.Cells[recordIndex, 4].Value = data.CardBrand;
        //            workSheet.Cells[recordIndex, 5].Value = data.CardMethod;
        //            workSheet.Cells[recordIndex, 6].Value = responseData.Error;
        //            workSheet.Cells[recordIndex, 7].Value = responseData.ErrorDescription;

        //            if (responseData.Error != null && responseData.Error.Equals("INVALID_REQUEST", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                invalidRequestCount++;
        //            }
        //            recordIndex++;
        //        }
        //    }

        //    workSheet2.Cells[2, 6].Value = invalidRequestCount;
        //    workSheet3.Cells[2, 1].Value = batchData.TextFileAmount;
        //    workSheet3.Cells[2, 2].Value = successTotalAmount;
        //    workSheet3.Cells[2, 3].Value = batchData.TextFileAmount - successTotalAmount;

        //    workSheet.Cells.AutoFitColumns();
        //    workSheet2.Cells.AutoFitColumns();
        //    workSheet3.Cells.AutoFitColumns();

        //    string outputFileName = FileServices.GetFileNameWithoutExt(batchData.FileName).Remove(7, 1).Insert(7, "R");
        //    string name = outputFileName + "batchreconsolidationreport.xlsx";
        //    string path = Report + name;
        //    FileServices.WriteToExcel(path, excel.GetAsByteArray());
        //    excel.Dispose();

        //    try
        //    {
        //        EmailServices.SendEmailDataReconsolidation("Batch billing reconsolidation report", $"{name} has been generated successfully.", ConfigurationHelper.GetValue("ToEmail"), path);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.Error(String.Format("Failed to send data reconsolidation email", ex));
        //    }
        //}
    }
}
