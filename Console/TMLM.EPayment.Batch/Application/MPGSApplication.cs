using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class MPGSApplication : BatchApplication
    {
        private readonly string ApiHeader = "apiOperation,authentication.3ds.transactionId,authentication.3ds.acsEci,authenication.3ds.authenticationToken,authentication.3ds2.transactionStatus,authentication.3ds1.paResStatus,authentication.3ds1.veResEnrolled,order.id,transaction.id,order.reference,transaction.reference,order.amount,order.currency,sourceOfFunds.type,sourceOfFunds.provided.card.number,sourceOfFunds.provided.card.expiry.month,sourceOfFunds.provided.card.expiry.year,result,error.cause,error.explanation,response.gatewayCode,authorizationResponse.responseCode,transaction.authorizationCode";
        private string ContentData = "";
        private List<MPGSResponseModel> ListOfResponse = new List<MPGSResponseModel>();
        public MPGSApplication(DatabaseHelper db) : base(db) { }

        /// <summary>
        /// MGSP - Generate flat files
        /// </summary>
        public override void GenerateFile()
        {
            ContentData = ApiHeader;
            LogHelper.Info("Start Generate the information to specific API format");
            foreach (var data in ExtractDatas)
            {
                string month = ConfigurationHelper.GetValue("defaultMonth");
                if (string.IsNullOrEmpty(month))
                    month = data.ExpiryMonth;

                string year = ConfigurationHelper.GetValue("defaultYear");
                if (string.IsNullOrEmpty(year))
                    year = data.ExpiryYear;

                var guid = Guid.NewGuid().ToString().Replace("-", "");
                guid = guid.Substring(0, 6);

                data.BatchId = BatchId;

                _db.ExecuteNonQuery(GenerateQuery(data, guid));
                ContentData += $"\n{data.ApiOperation},{guid},{guid},{guid},{guid},{data.Amount},{data.Currency},{data.PaymentType},{data.AccountNumber},{month},{year},,,,,";
            }
            LogHelper.Info("Successfully Generate the information to specific API format");
            string outputPath = $"{Output}{FileName}-Api.txt";
            FileServices.WriteToFile(outputPath, ContentData);
        }

        private static string GenerateQuery(ExtractDataModel dataModel, string guid)
        {
            return $"INSERT INTO MPGSBatchProcessResult([BatchId], [merchantID] ,[RecordType] ,[MechantNumber] ,[AccountName] ,[AccountAddress] ,[PhoneNUmber], [PolicyNumber],[PostCode] ,[SumAssured] ,[orderID] ,[transactionID] ,[amount] ,[currency] ,[bankCard] ,[bankExpiry] ,[result] ,[errorCause] ,[errorDescription] ,[gatewayCode] ,[apiOperation] ,[CreateDate] ,[LastUpdated]) " +
                $"VALUES ({dataModel.BatchId}, '{dataModel.MerchantId}','{dataModel.RecordType}','{dataModel.MerchantNumber}','{dataModel.AccountName}','{dataModel.Address}','{dataModel.PhoneNumber}','{dataModel.PolicyNumber}','{dataModel.PostCode}','{dataModel.SumAssured}','{guid}','{guid}','{dataModel.Amount}','{dataModel.Currency}','{UtilHelper.MaskCardNumber(dataModel.AccountNumber)}','{dataModel.ExpiryDate}','INITIAL','N/A','N/A','N/A','{dataModel.ApiOperation}','{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}','{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}')";
        }

        public override async Task StartRestApi()
        {
            LogHelper.Info("Initial Configure the Rest API");
            UpdateBatchStatus("Initial", "Ready for Rest API process");
            var baseUrl = ConfigurationHelper.GetValue("mpgs.baseUrl");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + ConfigurationHelper.GetValue("mpgs.merchant.password")));
            var encodingType = ConfigurationHelper.GetValue("mpgs.encodingType");
            var version = ConfigurationHelper.GetValue("mpgs.api.version");
            var merchantId = ConfigurationHelper.GetValue("mpgs.merchant.id");
            var information = ConfigurationHelper.GetValue("mpgs.url-information").Replace("{version}", version);
            var localUniqueId = Guid.NewGuid().ToString().Replace("-", "");
            var batch = ConfigurationHelper.GetValue("mpgs.url-batch").Replace("{version}", version).Replace("{merchantid}", merchantId).Replace("{uniqueid}", localUniqueId);
            var response = ConfigurationHelper.GetValue("mpgs.url-response");
            var status = ConfigurationHelper.GetValue("mpgs.url-status");
            var validate = ConfigurationHelper.GetValue("mpgs.url-validation");
            var contentType = ConfigurationHelper.GetValue("mpgs.ContentType");

            LogHelper.Info("Successfully Configure the Rest API");
            using (var apiHelper = new RestApiHelper<MPGSResponseModel>(baseUrl, password, encodingType, contentType))
            {
                LogHelper.Info("Ping Rest API Process for MPGS");
                var checkAlive = await apiHelper.PingServerAsync(information);

                UpdateBatchStatus("Ping", "Check MPGS Service Online?");
                LogHelper.Info("Start Rest API Process for MPGS");

                if (checkAlive)
                {
                    LogHelper.Info("The MPGS services is Alive. Initiate the batch process");
                    UpdateBatchStatus("Upload", "Upload Batch file to MPGS");
                    var uploadBatch = await apiHelper.CallRestApiString(batch, HttpMethod.Put, ContentData, encodingType);
                    if (uploadBatch.Contains(ApiStatus.Fail))
                    {
                        LogHelper.Info("File Fails to Upload the MPGS API");
                        return;
                    }

                    LogHelper.Info("Successfully upload the batch.");
                    UpdateBatchStatus("Success", "Success Upload Batch file to MPGS");
                    while (!uploadBatch.Contains(ApiStatus.Upload))
                    {
                        uploadBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                    }
                    LogHelper.Info("Generate validate code");
                    UpdateBatchStatus("Validate", "Validate Uploaded Batch file in MPGS");

                    var HashContent = EncrpytionDescrpytion.EncryptString(ContentData);
                    LogHelper.Info("Validate the batch process"); LogHelper.Info("Validate the batch process");
                    var validateBatch = await apiHelper.CallRestApiString($"{batch}{validate}", HttpMethod.Post, HashContent, encodingType);

                    LogHelper.Info("Validate Success. MPGS start process the batch.");
                    UpdateBatchStatus("Success", "Validate Success Uploaded Batch file in MPGS");

                    var statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                    while (!statusBatch.Contains(ApiStatus.Complete))
                    {
                        LogHelper.Info("MPGS Batch Service currently still processing the information...");
                        statusBatch = await apiHelper.CallRestApiString($"{batch}{status}", HttpMethod.Get, "", encodingType);
                        await Task.Delay((ExtractDatas.Count * 1000));
                    }

                    LogHelper.Info("MPGS Batch Service finish process the batch.");
                    UpdateBatchStatus("Processed", "MPGS processed the batch file");

                    LogHelper.Info("Fetching the batch information.");
                    var result = await apiHelper.CallRestApiString($"{batch}{response}", HttpMethod.Get, "", encodingType);

                    LogHelper.Info("Store the batch information result. \n \n");
                    StoreResult(result);
                }
            };
        }

        private void StoreResult(string content)
        {
            var arrString = content.Split('\n');
            var result = ApiHeader;
            for (var i = 0; i < arrString.Length; i++)
            {
                if (!arrString[i].ToLower().Contains("apioperation"))
                {
                    var arrResult = arrString[i].Split(new[] { '\"', ',' });
                    result += $"\n";    
                    for (var j = 1; j < arrResult.Length; j += 3)
                    {
                        if (j != arrResult.Length - 2)
                            result += $"{arrResult[j]},";
                        else
                            result += $"{arrResult[j]}";
                    }

                    MPGSResponseModel model = new MPGSResponseModel()
                    {
                        ApiOperation = arrResult[1],
                        OrderId = arrResult[4],
                        TransactionId = arrResult[7],
                        OrderRef = arrResult[10],
                        TransactionRef = arrResult[13],
                        Amount = arrResult[16],
                        Currency = arrResult[19],
                        Type = arrResult[22],
                        BankNumber = arrResult[25],
                        ExpiryMonth = arrResult[28],
                        ExpiryYear = arrResult[31],
                        Result = arrResult[34],
                        Error = arrResult[37],
                        ErrorDescription = arrResult[40] + arrResult[43],
                        GatewayCode = arrResult[46],
                        AuthorizationCode = arrResult[49],
                        Source = arrString[i]
                    };
                    ListOfResponse.Add(model);
                }
            }
            UpdateMPGSBatchInformation();
        }

        private void UpdateMPGSBatchInformation()
        {
            foreach (var model in ListOfResponse)
            {
                _db.ExecuteNonQuery($"UPDATE MPGSBatchProcessResult SET RESULT = '{model.Result}', errorCause = '{model.Error}', errorDescription = '{model.ErrorDescription}', gatewayCode = '{model.GatewayCode}', LastUpdated = '{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}', bankCard = '{UtilHelper.MaskCardNumber(model.BankNumber)}',AuthorizationCode = '{model.AuthorizationCode}', Source = '{model.Source}' WHERE orderID = '{model.OrderId}' ");
            }
        }

        public void GenerateResultFile()
        {
            var content = $"{RecordHeader},{ApiHeader}";
            var outputSuccessResult = Header;// Header.Insert(5,"0");
            var outputFailResult = Header;
            var RefNumber = 200340010000;
            int successRecordCount = 0;
            int failRecordCount = 0;
            int successTotalAmount = 0;
            int failTotalAmount = 0;
            string outputFileName = FileName.Remove(7, 1);
            outputFileName = outputFileName.Insert(7, "R");
            string outputPath = $"{Output}{outputFileName}";
            for (var i = 0; i < ListOfResponse.Count; i++)
            {
                content = $"\n{ExtractDatas[i].MerchantId},{ExtractDatas[i].RecordType},{ExtractDatas[i].MerchantNumber},{ExtractDatas[i].PolicyNumber},{ListOfResponse[i].BankNumber},{ExtractDatas[i].ExpiryDate},{ExtractDatas[i].Amount},{ExtractDatas[i].AccountName},{ExtractDatas[i].Address},{ExtractDatas[i].PhoneNumber},{ExtractDatas[i].PostCode},{ExtractDatas[i].SumAssured},{ExtractDatas[i].ApiOperation},{ListOfResponse[i].OrderId},{ListOfResponse[i].TransactionId},{ListOfResponse[i].OrderRef},{ListOfResponse[i].TransactionRef},{ListOfResponse[i].Amount},{ListOfResponse[i].Currency},{ListOfResponse[i].Type},{ListOfResponse[i].BankNumber},{ListOfResponse[i].ExpiryMonth},{ListOfResponse[i].ExpiryYear},{ListOfResponse[i].Result},{ListOfResponse[i].Error},{ListOfResponse[i].ErrorDescription},{ListOfResponse[i].GatewayCode}";
                var APRCODE = ListOfResponse[i].AuthorizationCode; //6
                var RESCODE = ListOfResponse[i].GatewayCode; //2
                var RESCODE2 = RefNumber; //12
                var BATCHNUM = "0" + Header.Substring(5, 5); //6
                RefNumber++;
                var Amount = ExtractDatas[i].Amount.Replace(".", "");
                for (var j = Amount.Length; Amount.Length < 11; j++)
                {
                    Amount = Amount.Insert(0, "0");
                }
                if (ListOfResponse[i].ErrorDescription.Equals("APPROVED"))
                {
                    outputSuccessResult += $"{Environment.NewLine}{ExtractDatas[i].MerchantId}{ExtractDatas[i].RecordType}{ExtractDatas[i].MerchantNumber}{ExtractDatas[i].PolicyNumber}{ExtractDatas[i].AccountNumber}{ExtractDatas[i].ExpiryDate}{Amount}{ExtractDatas[i].AccountName}{ExtractDatas[i].Address}{ExtractDatas[i].PhoneNumber}{ExtractDatas[i].PostCode}{ExtractDatas[i].SumAssured}{APRCODE}{RESCODE}{RESCODE2}{BATCHNUM}";
                    successRecordCount++;
                    successTotalAmount += Convert.ToInt32(Convert.ToDecimal(ExtractDatas[i].Amount));
                }
                else
                {
                    outputFailResult += $"{Environment.NewLine}{ExtractDatas[i].MerchantId}{ExtractDatas[i].RecordType}{ExtractDatas[i].MerchantNumber}{ExtractDatas[i].PolicyNumber}{ExtractDatas[i].AccountNumber}{ExtractDatas[i].ExpiryDate}{Amount}{ExtractDatas[i].AccountName}{ExtractDatas[i].Address}{ExtractDatas[i].PhoneNumber}{ExtractDatas[i].PostCode}{ExtractDatas[i].SumAssured}{APRCODE}{RESCODE}{RESCODE2}{BATCHNUM}";
                    failRecordCount++;
                    failTotalAmount += Int32.Parse(ExtractDatas[i].Amount.Replace(".00", ""));
                }
            }
            var strSuccessAmount = successTotalAmount.ToString() + "00";
            var strFailAmount = failTotalAmount.ToString() + "00";
            var strSuccessRecordCount = successRecordCount.ToString();
            var strFailRecordCount = failRecordCount.ToString();

            for (var i = strSuccessAmount.Length; i < 13; i++)
            {
                strSuccessAmount = strSuccessAmount.Insert(0, "0");
            }
            for (var i = strFailAmount.Length; i < 13; i++)
            {
                strFailAmount = strFailAmount.Insert(0, "0");
            }
            for (var i = strFailRecordCount.Length; i < 5; i++)
            {
                strFailRecordCount = strFailRecordCount.Insert(0, "0");
            }
            for (var i = strSuccessRecordCount.Length; i < 5; i++)
            {
                strSuccessRecordCount = strSuccessRecordCount.Insert(0, "0");
            }
            var SuccessFooter = $"{Header}{strSuccessRecordCount}{strSuccessAmount}";
            var FailFooter = $"{Header}{strFailRecordCount}{strFailAmount}";
            outputSuccessResult += $"{Environment.NewLine}{SuccessFooter}";
            outputFailResult += $"{Environment.NewLine}{FailFooter}";
            FileServices.WriteToFile(outputPath + "-Result.csv", content);
            FileServices.WriteToFile(outputPath + "A1.txt", outputSuccessResult);
            FileServices.WriteToFile(outputPath + "D1.txt", outputFailResult);

            var processPath = $"{Processing}/{FileName}";
            var competePath = $"{Completed}/{FileName}.txt";
            FileServices.MoveFile(processPath, competePath);

            UpdateBatchStatus("Complete", "Successful perform all the Batch Process");


            string[] toEmailArray = ConfigurationHelper.GetValue("ToEmail").Split(',');
            foreach (string toEmail in toEmailArray)
            {
                EmailServices.SendEmail("Batch Process Complete Notification", $"{FileName}.TXT has been process complete. \n Total Data in File Count : {ExtractDatas.Count} || Total Return Count : {ListOfResponse.Count} \n Number of success record : {successRecordCount} || Number of failed record : {failRecordCount}", toEmail, outputPath + "A1.txt", outputPath + "D1.txt");
            }
        }
    }
}
