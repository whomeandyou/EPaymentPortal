using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using TMLM.EPayment.Batch.Model;
using TMLM.EPayment.Batch.Helpers;

namespace TMLM.EPayment.Batch.Data
{
    public enum BatchStatus
    {
        DataExtraction,
        DataInitilization,
        MPGSServicePing,
        MPGSServiceFileUpload,
        MPGSServiceValidateUploadedRequest,
        MPGSServiceBatchValidation,
        MPGSServiceBatchStatus,
        MPGSServiceRetriveProcessedResult, 
        Completed
    }

    public class BatchPaymentRepo: IDisposable
    {
        protected readonly SqlConnection Conn;

        public BatchPaymentRepo(string dbConnectionString)
        {
            this.Conn = new SqlConnection(dbConnectionString);
        }

        public IDbConnection Open()
        {
            Conn.Open();
            return Conn;
        }

        public async Task<IDbConnection> OpenAsync()
        {
            await Conn.OpenAsync();
            return Conn;
        }

        public BatchProcess HasHistRecord(string filename)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
SELECT 
    [BatchID]
    ,[FileName]
    ,[Status]
    ,[StatusDescription]
    ,[CreateDate]
    ,[LastUpdated]
    ,[StatusId]
    ,[IsRecurringBatch]
    ,[UrlGuId]
    ,[ProcessingStatus]
    ,[BatchData]
    ,[DataContent]
FROM
	BatchProcess
WHERE 
    filename = @Filename
";
            dParams.Add("@Filename", filename);

            return this.Conn.QueryFirstOrDefault<BatchProcess>(new CommandDefinition(query, dParams));
        }

        public BatchProcess GetBatchById(int batchId)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
SELECT 
    [BatchID]
    ,[FileName]
    ,[Status]
    ,[StatusDescription]
    ,[CreateDate]
    ,[LastUpdated]
    ,[StatusId]
    ,[IsRecurringBatch]
    ,[UrlGuId]
    ,[ProcessingStatus]
    ,[BatchData]
    ,[DataContent]
FROM
	BatchProcess
WHERE
    BatchID = @BatchID
";
            dParams.Add("@BatchID", batchId);

            return this.Conn.QueryFirstOrDefault<BatchProcess>(new CommandDefinition(query, dParams));
        }

        public List<SummaryInfo> GetSummaryResult(int batchId)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT [CardType],[CardMethod],[GatewayCode], SUM(Amount) AS Amount, Count(*) AS Total FROM MPGSBatchProcessResult WHERE BatchID = @BatchID GROUP BY [CardType],[CardMethod],[GatewayCode]";
            dParams.Add("@BatchID", batchId);


            //return (List<SummaryInfo>)x;
            return this.Conn.Query<SummaryInfo>(new CommandDefinition(query, dParams)).AsList();
        }

        public BatchProcess UpdateBatchProcessStatus(BatchProcess data, PaymentRequestModel reqData, BatchStatus status, string additionalInfo = "")
        {
            if (reqData != null)
            {
                reqData.Status = String.IsNullOrEmpty(reqData.Status) ? status.ToString(): reqData.Status;
                InsertBatchTransaction(reqData);
            }
           
            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
UPDATE 
    [BatchProcess] 
SET 
    status = @status, 
    statusDescription = @statusDescription, 
    LastUpdated = GETDATE() 
WHERE 
    batchID = @batchID
";
            dParams.Add("@status", status.ToString());
            dParams.Add("@statusDescription", additionalInfo);
            dParams.Add("@batchID", data.BatchID);
            dParams.Add("@batchID", data.BatchID);

            return this.Conn.ExecuteScalar<BatchProcess>(new CommandDefinition(query, dParams));
        }

        public int InsertBatchHeader(BatchProcess data)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"INSERT INTO BatchProcess ([Filename],[Status],[StatusDescription],[CreateDate],[LastUpdated]) "
                           + "OUTPUT INSERTED.[BatchID],INSERTED.[Filename],INSERTED.[StatusDescription],INSERTED.[CreateDate],INSERTED.[LastUpdated] "
                           + "VALUES (@Filename,@Status,@StatusDescription,GETDATE(),GETDATE());";

            dParams.Add("@Filename", data.FileName);
            dParams.Add("@Status", data.Status);
            dParams.Add("@StatusDescription", data.StatusDescription);

            return (int)this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public object InsertBatchDetails(ExtractDataModel data)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"INSERT INTO [MPGSBatchProcessResult] ([BatchID],[merchantID],[RecordType],[MechantNumber],[AccountName],[AccountAddress],[PhoneNumber],[PolicyNumber],[PostCode],[SumAssured]"
                           + " ,[OrderID],[TransactionID],[Amount],[Currency],[BankCard],[BankExpiry],[Result],[ErrorCause],[ErrorDescription],[GatewayCode]" 
                           + " ,[ApiOperation],[CreateDate],[LastUpdated],[AuthorizationCode],[Source],[AgreementId])"
                           + " OUTPUT"
                           + " INSERTED.[ProcessID],INSERTED.[BatchID],INSERTED.[MerchantID],INSERTED.[RecordType],INSERTED.[MechantNumber],INSERTED.[AccountName],INSERTED.[AccountAddress],INSERTED.[PhoneNumber],INSERTED.[PolicyNumber],INSERTED.[PostCode], "
                           + " INSERTED.[SumAssured],INSERTED.[OrderID],INSERTED.[TransactionID],INSERTED.[Amount],INSERTED.[Currency],INSERTED.[BankCard],INSERTED.[BankExpiry],INSERTED.[Result],INSERTED.[ErrorCause],INSERTED.[ErrorDescription], "
                           + " INSERTED.[GatewayCode],INSERTED.[ApiOperation],INSERTED.[CreateDate],INSERTED.[LastUpdated],INSERTED.[AuthorizationCode],INSERTED.[Source],INSERTED.[AgreementId]"
                           + " VALUES ("
                           + "         @BatchId,@merchantID,@RecordType,@MechantNumber,@AccountName,@AccountAddress,@PhoneNumber,@PolicyNumber,@PostCode,@SumAssured"
                           + "         ,@OrderID,@TransactionID,@Amount,@Currency,@BankCard,@BankExpiry,@Result,@ErrorCause,@ErrorDescription,@GatewayCode"
                           + "         ,@ApiOperation,GETDATE(),GETDATE(),@AuthorizationCode,@Source,@AgreementId"
                           + " );";

            dParams.Add("@BatchId", data.BatchId);
            dParams.Add("@merchantID", data.MerchantId);
            dParams.Add("@RecordType", data.RecordType);
            dParams.Add("@MechantNumber", data.MerchantNumber);
            dParams.Add("@AccountName", data.AccountName);
            dParams.Add("@AccountAddress", data.Address);
            dParams.Add("@PhoneNumber", data.PhoneNumber);
            dParams.Add("@PolicyNumber", data.PolicyNumber);
            dParams.Add("@PostCode", data.PostCode);
            dParams.Add("@SumAssured", data.SumAssured);

            dParams.Add("@OrderID", data.OrderId);
            dParams.Add("@TransactionID", data.OrderId);
            dParams.Add("@Amount", data.Amount);
            dParams.Add("@Currency", data.Currency);
            dParams.Add("@BankCard", UtilHelper.MaskCardNumber(data.AccountNumber));
            dParams.Add("@BankExpiry", data.ExpiryDate);
            dParams.Add("@Result", "NEW");
            dParams.Add("@ErrorCause", "");
            dParams.Add("@ErrorDescription", "");
            dParams.Add("@GatewayCode", "");

            dParams.Add("@ApiOperation", data.ApiOperation);
            dParams.Add("@AuthorizationCode", "");
            dParams.Add("@Source", "");
            dParams.Add("@AgreementId", data.AgreementId);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public object InsertBatchTransaction(PaymentRequestModel data)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"INSERT INTO PaymentRequestLog ([BatchId],[Status],[RequestUrl],[RequestBody],[ResponseBody]) "
                           + "OUTPUT INSERTED.[Id],INSERTED.[BatchId],INSERTED.[Status],INSERTED.[RequestUrl],INSERTED.[ResponseBody] "
                           + "VALUES (@BatchId,@Status,@RequestUrl,@RequestBody,@ResponseBody);";

            dParams.Add("@BatchId", data.BatchId);
            dParams.Add("@Status", data.Status);
            dParams.Add("@RequestUrl", data.RequestUrl);
            dParams.Add("@RequestBody", data.RequestBody);
            dParams.Add("@ResponseBody", data.ResponseBody);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public object UpdateBatchPaymentUrl(int batchId, string GUID, bool isRecurringBatch)
        {
            int isRecurringBatchI = isRecurringBatch ? 1 : 0;

            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
UPDATE 
	BatchProcess 
SET 
	UrlGuId = @UrlGuId,
    IsRecurringBatch = @isRecurringBatch
WHERE 
	BatchID = @BatchID
";
            dParams.Add("@UrlGuId", GUID);
            dParams.Add("@BatchID", batchId);
            dParams.Add("@isRecurringBatch", isRecurringBatchI);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public object UpdateBatchData(int batchId, string dataContent, string uploadBatch)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
UPDATE 
	BatchProcess 
SET 
	DataContent = @dataContent,
    BatchData = @uploadBatch
WHERE 
	BatchID = @BatchID
";
            dParams.Add("@BatchId", batchId);
            dParams.Add("@dataContent", dataContent);
            dParams.Add("@uploadBatch", uploadBatch);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }


        public object UpdateBatchProcessingStatus(int batchId, string processingStatus, BatchStatus status)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $@"
UPDATE 
	BatchProcess 
SET 
	ProcessingStatus = @processingStatus,
    StatusId = @StatusId
WHERE 
	BatchID = @BatchID
";
            dParams.Add("@BatchID", batchId);
            dParams.Add("@processingStatus", processingStatus);
            dParams.Add("@StatusId", (int)status);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public void UpdateTransactionDetails(List<MPGSResponseModel> responseData)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"UPDATE MPGSBatchProcessResult SET " +
                           $"RESULT=@Result, errorCause=@Error, errorDescription=@ErrorDesc, gatewayCode=@GatewayCode, LastUpdated=@LastUpdateDate, bankCard=@BankNumber, AuthorizationCode=@AuthorizationCode,Source=@Source,CardType=@CardType,CardMethod=@CardMethod " +
                           $"WHERE orderID = @OrderId ";

            foreach (var data in responseData)
            {
                if (!data.IsUpdated)
                {
                    dParams = new DynamicParameters();
                    dParams.Add("@Result", data.Result);
                    dParams.Add("@Error", data.Error);
                    dParams.Add("@GatewayCode", data.GatewayCode);
                    dParams.Add("@ErrorDesc", data.ErrorDescription);
                    dParams.Add("@LastUpdateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dParams.Add("@BankNumber", data.BankNumber);
                    dParams.Add("@AuthorizationCode", data.AuthorizationCode);
                    dParams.Add("@Source", data.Source);
                    dParams.Add("@OrderId", data.OrderId);
                    dParams.Add("@CardType", data.CardBrand);
                    dParams.Add("@CardMethod", data.CardMethod);

                    this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
                    data.IsUpdated = true;
                }
            }
        }

        public MPGSBatchProcessResult GetTransactionDetailsByBatchId(int batchId, ExtractDataModel data)
        {   
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT "
                           + " ProcessID,[BatchId],[merchantID],[RecordType],[MechantNumber],[AccountName],[AccountAddress],[PhoneNumber],[PolicyNumber],[PostCode],[SumAssured]"
                           + " ,[OrderID],[TransactionID],[Amount],[Currency],[BankCard],[BankExpiry],[Result],[ErrorCause],[ErrorDescription],[GatewayCode]"
                           + " ,[ApiOperation],[CreateDate],[LastUpdated],[AuthorizationCode],[Source],[AgreementId]"
                           + "FROM MPGSBatchProcessResult WHERE BatchID=@BatchID AND PolicyNumber=@PolicyNumber AND AgreementId=@AgreementId AND IsActive=1 ";
            dParams.Add("@BatchId", batchId);
            dParams.Add("@PolicyNumber", data.PolicyNumber);
            dParams.Add("@AgreementId", data.AgreementId);
            //dParams.Add("@Status", "SUCCESS");

            return this.Conn.QueryFirstOrDefault<MPGSBatchProcessResult>(new CommandDefinition(query, dParams));
        }

        public object UpdateTransactionDetailsInActive(int batchId, int processId)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"UPDATE MPGSBatchProcessResult SET IsActive = 0 WHERE ProcessID=@ProcessID";
            dParams.Add("@BatchId", batchId);
            dParams.Add("@processId", processId);

            return this.Conn.ExecuteScalar(new CommandDefinition(query, dParams));
        }

        public MPGSBatchProcessResult HasSuccessTransaction(ExtractDataModel data)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT "
                           + " ProcessID,[BatchId],[merchantID],[RecordType],[MechantNumber],[AccountName],[AccountAddress],[PhoneNumber],[PolicyNumber],[PostCode],[SumAssured]"
                           + " ,[OrderID],[TransactionID],[Amount],[Currency],[BankCard],[BankExpiry],[Result],[ErrorCause],[ErrorDescription],[GatewayCode]"
                           + " ,[ApiOperation],[CreateDate],[LastUpdated],[AuthorizationCode],[Source],[AgreementId]"
                           + "FROM MPGSBatchProcessResult WHERE PolicyNumber=@PolicyNumber AND AgreementId=@AgreementId AND result=@Status";

            dParams.Add("@PolicyNumber", data.PolicyNumber);
            dParams.Add("@AgreementId", data.AgreementId);
            dParams.Add("@Status", "SUCCESS");

            return this.Conn.QueryFirstOrDefault<MPGSBatchProcessResult>(new CommandDefinition(query, dParams));
        }

        public List<MPGSBatchProcessSuccessModel> SuccessTransactionList()
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT "
                           + " [PolicyNumber],[AgreementId],[Result] "
                           + " FROM MPGSBatchProcessResult WHERE result=@Status "
                           + " group by PolicyNumber,AgreementId,Result";

            dParams.Add("@Status", "SUCCESS");

            return this.Conn.Query<MPGSBatchProcessSuccessModel>(new CommandDefinition(query, dParams)).AsList();
        }

        public bool CheckMPGSStatus(int batchID)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT "
                           + " TOP 1 1"
                           + "FROM MPGSBatchProcessResult WHERE batchID=@BatchID and result= @Result and isactive = 1";

            dParams.Add("@BatchID", batchID);
            dParams.Add("@Result", "NEW");

            return this.Conn.ExecuteScalar<bool>(new CommandDefinition(query, dParams));
        }

       

        public void Close()
        {
            if (this.Conn.State == ConnectionState.Open)
                this.Conn.Close();
        }

        public void Dispose()
        {
            if (this.Conn.State == ConnectionState.Open)
                this.Conn.Close();

            Conn.Dispose();
        }
    }
}
