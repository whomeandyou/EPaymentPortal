using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Application;
using TMLM.EPayment.Batch.Data;
using TMLM.EPayment.Batch.Helpers;
using TMLM.EPayment.Batch.Model;
using TMLM.EPayment.Batch.Service;

namespace TMLM.EPayment.Batch.Abstract
{
    public abstract class BatchApplication 
    {
        internal List<ExtractDataModel> ExtractDatas;

        private readonly static string Root = ConfigurationHelper.GetValue("file.root.path");
        public readonly static string Output = ConfigurationHelper.GetValue("file.output.path");
        private readonly static string Upload = ConfigurationHelper.GetValue("file.upload.path");
        internal readonly static string Processing = ConfigurationHelper.GetValue("file.processing.path");
        private readonly static string Failed = ConfigurationHelper.GetValue("file.failed.path");
        internal readonly static string Completed = ConfigurationHelper.GetValue("file.completed.path");
        public readonly static string Report = ConfigurationHelper.GetValue("file.report.path");

        internal int BatchId = 0;
        internal string UniqueId = Guid.NewGuid().ToString().Replace("-", "");
        internal string RecordHeader = "Merchant Id,Record Type, Merchant Number, Policy Number, Account Number, Expiry Date, Amount, Account Name, Address, Phone Number, PostCode, Sum Assured";
        internal string FileName = "";
        public string Header = "";
        public string Footer = "";

        public readonly DatabaseHelper _db;
        public readonly BatchPaymentRepo _repo;
        public readonly EPaymentRepo _repoEPayment;

        public BatchApplication(DatabaseHelper db)
        {
            this._db = db;
            UniqueId = UniqueId.Substring(0, 6);
        }

        #region New Implementation 
        public BatchApplication(BatchPaymentRepo repo,EPaymentRepo repoEpayment)
        {
            _repo = repo;
            _repoEPayment = repoEpayment;
        }

        public virtual BatchProcess DataFileExtration(string fullFilePath)
        {
            LogHelper.Info($"BatchApplication.DataFileExtration :=> Filename: {fullFilePath}.");

            this.FileName = FileServices.GetFileName(fullFilePath); 
            BatchProcess histBatchData = _repo.HasHistRecord(this.FileName);

            String[] arrContent = FileServices.ReadStringFromFile(fullFilePath);
            if (arrContent != null && arrContent.Length > 0)
            {
                Header = arrContent.FirstOrDefault();
                Footer = arrContent.LastOrDefault();

                var successTransactionList = _repo.SuccessTransactionList();

                // 1. No historical records
                if (histBatchData == null)
                {
                    var batchId = _repo.InsertBatchHeader(new BatchProcess() { FileName=this.FileName, Status = BatchStatus.DataExtraction.ToString(), StatusDescription="Create new record" });
                    _repo.UpdateBatchProcessingStatus(batchId, "Processing", Data.BatchStatus.MPGSServiceFileUpload);
                    histBatchData = _repo.GetBatchById(batchId);
                    histBatchData.ExtractedData = ExtractDataHelper.ExtractInformation(histBatchData.BatchID, arrContent, _repoEPayment);

                    foreach (ExtractDataModel data in histBatchData.ExtractedData)
                    {

                        var batchTransId = _repo.InsertBatchDetails(data);
                        data.ProcessID = (int)batchTransId;

                        if (successTransactionList != null)
                        {
                            var hasSuccessTransactedRecord =
                                successTransactionList.Where(x => x.AgreementId == data.AgreementId && x.PolicyNumber == data.PolicyNumber).ToList();
                            data.HasSucessTransactedRecord = hasSuccessTransactedRecord.Count > 0 ? true : false;
                        }
                    }
                }
                else
                {
                    _repo.UpdateBatchProcessStatus(histBatchData, null, BatchStatus.DataExtraction, "Re-process existing data");
                    histBatchData.ExtractedData = ExtractDataHelper.ExtractInformation(histBatchData.BatchID, arrContent, _repoEPayment);
                    foreach (ExtractDataModel data in histBatchData.ExtractedData)
                    {
                        MPGSBatchProcessResult hasExistingData = _repo.GetTransactionDetailsByBatchId(histBatchData.BatchID, data);
                        if(hasExistingData != null)
                        {
                            data.OrderId = hasExistingData.OrderID;
                            if (hasExistingData.Result != "NEW")
                            {
                                data.Result = hasExistingData.Result;
                                data.GatewayCode = hasExistingData.GatewayCode;
                                data.ResponseData = hasExistingData.Source;
                                data.AuthorizationCode = hasExistingData.AuthorizationCode;
                            }
                            //else
                            //{
                            //    _repo.UpdateTransactionDetailsInActive(histBatchData.BatchID, hasExistingData.ProcessID);
                            //    var batchTransId = _repo.InsertBatchDetails(data);
                            //    data.ProcessID = (int)batchTransId;
                            //}
                        }
                        else
                        {
                            var batchTransId = _repo.InsertBatchDetails(data);
                            data.ProcessID = (int)batchTransId;
                        }

                        if (successTransactionList != null)
                        {
                            var hasSuccessTransactedRecord =
                                successTransactionList.Where(x => x.AgreementId == data.AgreementId && x.PolicyNumber == data.PolicyNumber).ToList();
                            data.HasSucessTransactedRecord = hasSuccessTransactedRecord.Count > 0 ? true : false;
                        }
                    }
                }
            }
            return histBatchData;
        }

        #endregion

        public virtual void GenerateFile() { }

        public virtual async Task StartRestApi() { }

        public virtual async Task StartRecurringRestApi() { }

        public bool CheckFolderExist()
        {
            LogHelper.Info($"System Validation on Directory.");
            return FileServices.DirectoryExist(new[] { Root, Output, Upload, Processing, Failed, Completed, Report});
        }

        public void ProcessInitialFile(string filePath)
        {
            LogHelper.Info($"Start Process Data in {filePath}.");

            List<string> lines = new List<string>();
            FileName = FileServices.GetFileName(filePath);

            LogHelper.Info($"Extract Data from file : {FileName}");
            foreach (var line in FileServices.ReadStringFromFile(filePath))
            {
                lines.Add(line);
            }

            LogHelper.Info($"Successful extract Data from file");
            Header = lines.FirstOrDefault();
            Footer = lines.LastOrDefault();

            var dataTable = _db.ExecuteReader($"select [BatchID],[FileName],[Status],[StatusDescription],[CreateDate],[LastUpdated] from BatchProcess where filename like '{FileName}'");
            if(dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                BatchProcess batch = new BatchProcess()
                {
                    BatchID = int.Parse(Convert.ToString(row[0])),
                    FileName = row[1].ToString(),
                    Status = row[2].ToString(),
                    StatusDescription = row[3].ToString(),
                    CreateDate = DateTime.Parse(row[4].ToString()),
                    LastUpdate = DateTime.Parse(row[5].ToString())
                };
                BatchId = batch.BatchID;

                UpdateBatchStatus("Re-Run",$"Re-Process the {FileName}");
                LogHelper.Info($"Re Process the file : {FileName}");

                ExtractDatas = ExtractDataHelper.ExtractInformationValidation(lines, _db);
                LogHelper.Info($"Successful process data to Data Object");
            }
            else
            {
                BatchId = _db.ExecuteScalar($"INSERT INTO BatchProcess ([fileName] ,[status] ,[statusDescription] ,[CreateDate] ,[LastUpdated]) VALUES ('{FileName}','Extract','Extract Data From File','{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}','{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}'); SELECT @@IDENTITY AS 'Identity'; ");
                LogHelper.Info($"Start process data to Data Object");

                //Detail information extraction 
                ExtractDatas = ExtractDataHelper.ExtractInformation(BatchId, lines.ToArray(), _repoEPayment);

                LogHelper.Info($"Successful process data to Data Object");
            }
            var processPath = $"{Processing}/{FileName}";
            FileServices.MoveFile(filePath, processPath);
        }

        public void UpdateBatchStatus(string status, string statusDescription)
        {
            _db.ExecuteNonQuery($" UPDATE [BatchProcess] set status = '{status}', statusDescription = '{statusDescription}', LastUpdated = '{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}'  where batchID = {BatchId}");
        }

    }
}
