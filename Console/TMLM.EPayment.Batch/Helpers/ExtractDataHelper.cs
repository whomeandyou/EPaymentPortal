using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Data;
using TMLM.EPayment.Batch.Model;

namespace TMLM.EPayment.Batch.Helpers
{
    public static class ExtractDataHelper
    {
        public static List<ExtractDataModel> ExtractInformation(int batchId, string[] lstdata, EPaymentRepo repoEpayment)
        {
            var ListData = new List<ExtractDataModel>();
            int iCounter = 0;

            foreach(var data in lstdata)
            {
                if (iCounter != 0 && iCounter != lstdata.Length - 1 )
                {
                    ListData.Add(ExtractTransactionInfo(batchId, data, iCounter, repoEpayment));
                }
                iCounter++;
            }

            return ListData;
        }

        public static List<ExtractDataModel> ExtractSummaryInformation(int batchId, string[] lstdata, BatchPaymentRepo repo)
        {
            var ListData = new List<ExtractDataModel>();
            int iCounter = 0;

            foreach (var data in lstdata)
            {
                if (iCounter != 0 && iCounter != lstdata.Length - 1)
                {
                    ListData.Add(ExtractSummaryTransactionInfo(batchId, data, iCounter, repo));
                }
                iCounter++;
            }

            return ListData;
        }

        public static List<ExtractDataModel> ExtractInformationValidation(List<string> datas, DatabaseHelper db)
        {
            var ListData = new List<ExtractDataModel>();
            int counter = 0;
            foreach (var data in datas)
            {
                if (counter != 0 && counter != datas.Count - 1)
                {
                    var amount = double.Parse(data.Substring(59, 11)).ToString();
                    ExtractDataModel dataModel = new ExtractDataModel()
                    {
                        MerchantId = data.Substring(0, 4),
                        RecordType = data.Substring(4, 1),
                        MerchantNumber = data.Substring(5, 9),
                        PolicyNumber = data.Substring(14, 25).Trim(),
                        AccountNumber = data.Substring(39, 16).Trim(),
                        ExpiryMonth = data.Substring(55, 2).Trim(),
                        ExpiryYear = data.Substring(57, 2).Trim(),
                        ExpiryDate = data.Substring(55, 4).Trim(),
                        Amount = amount.Substring(0, amount.Length - 2) + "." + amount.Substring(amount.Length - 2, 2),
                        AccountName = data.Substring(70, 26),
                        Address = data.Substring(96, 60).Replace(',', ' '),
                        PhoneNumber = data.Substring(156, 10),
                        PostCode = data.Substring(166, 5),
                        SumAssured = data.Substring(171, 12),
                        ApiOperation = "PAY",
                        Currency = "MYR",
                        PaymentType = "CARD" 
                    };

                    dataModel.AgreementId = String.Format("{0}{1}{2}{3}", dataModel.PolicyNumber, GetLast4Digits(dataModel.AccountNumber), dataModel.ExpiryYear, dataModel.ExpiryMonth);
                    var dt = db.ExecuteReader($"select ProcessID from MPGSBatchProcessResult where PolicyNumber = '{dataModel.PolicyNumber}' and AgreementId = '{dataModel.AgreementId}' and result = 'INITIAL'");
                    
                    if (dt.Rows.Count > 0)
                        ListData.Add(dataModel);
                }
                counter++;
            }
            return ListData;
        }

        private static ExtractDataModel ExtractTransactionInfo(int batchId, string content,int count, EPaymentRepo repoEpayment)
        {
            // Amount Format: 9988 ==> 99.88
            string amount = double.Parse(content.Substring(59, 11)).ToString();
            ExtractDataModel dataModel = new ExtractDataModel()
            {
                BatchId = batchId,
                MerchantId = content.Substring(0, 4),
                RecordType = content.Substring(4, 1),
                MerchantNumber = content.Substring(5, 9),
                PolicyNumber = content.Substring(14, 25).Trim(),
                AccountNumber = content.Substring(39, 16).Trim(),
                ExpiryMonth = content.Substring(55, 2).Trim(),
                ExpiryYear = content.Substring(57, 2).Trim(),
                ExpiryDate = content.Substring(55, 4).Trim(),
                Amount = amount.Substring(0, amount.Length - 2) + "." + amount.Substring(amount.Length - 2, 2),
                AccountName = content.Substring(70, 26),
                Address = content.Substring(96, 60).Replace(',', ' '),
                PhoneNumber = content.Substring(156, 10),
                PostCode = content.Substring(166, 5),
                SumAssured = content.Substring(171, 12),
                ApiOperation = "PAY",
                Currency = "MYR",
                PaymentType = "CARD"
            };

            string countString = string.Empty;
            if (count > 999)
            {
                countString = count.ToString().Substring(count.ToString().Length - 3, 3);
            }
            else
            {
                countString = count.ToString();
            }

            dataModel.AgreementId = String.Format("{0}{1}{2}{3}", dataModel.PolicyNumber, GetLast4Digits(dataModel.AccountNumber), dataModel.ExpiryYear, dataModel.ExpiryMonth);
            dataModel.OrderId = String.Format("{0:yyyyMMddHHmmss}{1}{2}{3:00000}{4:000}", DateTime.Now, dataModel.PolicyNumber, GetLast4Digits(dataModel.AccountNumber), dataModel.BatchId, countString);

            var dsData = repoEpayment.GetEnrollmentDetail(dataModel.PolicyNumber);

            if (dsData != null)
            {
                dataModel.Veres = dsData.Veres;
                dataModel.Pares = dsData.Pares;
                dataModel.dsAcsEci = dsData.AcsEci;
                dataModel.dsAuthenticationToken = dsData.AuthenticationToken;
                dataModel.dsTransactionId = dsData.TransactionId;
                dataModel.dsVersion = dsData.dsVersion;
            }

            return dataModel;
        }

        private static ExtractDataModel ExtractSummaryTransactionInfo(int batchId, string content, int count, BatchPaymentRepo repo)
        {
            Console.WriteLine(content);
            ExtractDataModel dataModel = new ExtractDataModel()
            { };
            return dataModel;
        }

        public static string GetLast4Digits(string accountNumber)
        {
            int strLength = 4;
            if (String.IsNullOrEmpty(accountNumber))
                return string.Empty;

            if (strLength >= accountNumber.Length)
                return accountNumber;

            return accountNumber.Substring(accountNumber.Length - strLength);
        }

        
    }
}
