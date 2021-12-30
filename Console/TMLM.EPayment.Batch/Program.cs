using System;
using System.Configuration;
using TMLM.EPayment.Batch.Application;
using TMLM.EPayment.Batch.Helpers;
using TMLM.EPayment.Batch.Service;
using TMLM.EPayment.Batch.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch
{
    class Program
    {
        private static readonly string DefaultConectionString = ConfigurationManager.ConnectionStrings["EPaymentBatch"].ConnectionString;
        private static readonly string ConectionStringEpayment = ConfigurationManager.ConnectionStrings["EPayment"].ConnectionString;
        private static readonly string UploadFolder = ConfigurationHelper.GetValue("file.upload.path");
        private static readonly string ProcessingFolder = ConfigurationHelper.GetValue("file.processing.path");

        static async Task Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();
            LogHelper.Info("Process Directory / File");

            using (Batch.Data.BatchPaymentRepo repo = new Data.BatchPaymentRepo(DefaultConectionString))
            {
                using (Batch.Data.EPaymentRepo repoEpayment = new Data.EPaymentRepo(ConectionStringEpayment))
                {
                    IMPGS batchMPGSPayment = new MPGSRecurring(repo, repoEpayment);

                    var isFolderReady = batchMPGSPayment.CheckFolderExist();
                    if (isFolderReady)
                    {
                        var files = FileServices.GetFilesPath(UploadFolder);
                        if (files != null && files.Count > 0)
                        {
                            foreach (var filePath in files)
                            {
                                batchMPGSPayment = new MPGSRecurring(repo, repoEpayment);

                                //Step 1: Data extraction 
                                BatchProcess batch = batchMPGSPayment.DataFileExtration(filePath);
                                batchMPGSPayment.ExtractExistingDataResponse(batch.ExtractedData);
                                // Move into processing folder
                                FileServices.MoveFile(filePath, $"{ProcessingFolder}/{FileServices.GetFileName(filePath)}");
                                bool task = true, taskRecurr = true;
                                if (batch != null && batch.ExtractedData != null && batch.ExtractedData.Count > 0)
                                {

                                    LogHelper.Info(String.Format("Info:=> Total records: {0}", batch.ExtractedData.Count));

                                    List<ExtractDataModel> lstFirtTimeBillingTransaction = batch.ExtractedData.FindAll(p => String.IsNullOrEmpty(p.ResponseData) & p.HasSucessTransactedRecord == false);
                                    if ((batch.ProcessingStatus == "Failed" && batch.IsRecurringBatch == false) || batch.ProcessingStatus == "Processing")
                                    {
                                        task = await batchMPGSPayment.RunFirstTimeBatchBilling(batch, lstFirtTimeBillingTransaction);

                                    }

                                    if (task)
                                    {
                                        if ((batch.ProcessingStatus == "Failed" && batch.IsRecurringBatch == true) || batch.ProcessingStatus == "Processing")
                                        {
                                            List<ExtractDataModel> lstRecurringBillingTransaction = batch.ExtractedData.FindAll(p => String.IsNullOrEmpty(p.ResponseData) & p.HasSucessTransactedRecord == true);
                                            taskRecurr = await batchMPGSPayment.RunRecurringBatchBilling(batch, lstRecurringBillingTransaction);

                                        }
                                    }

                                    if (task && taskRecurr)
                                    {
                                        // Generate Flat File
                                        batchMPGSPayment.GenerateFlatFile(batch);
                                        //batchMPGSPayment.GenerateReconsolidationReport(batch);
                                    }
                                }
                                else
                                {
                                    LogHelper.Info(String.Format("Info:=> No Record(s) Found. File Path: {0}", filePath));
                                }
                            }
                        }
                        else
                        {
                            LogHelper.Info($"There no file found in {UploadFolder}. The batch process has stop.");
                        }
                    }
                }

                LogHelper.Info("Batch Payment :=> Completed !!!");
#if DEBUG
                //Console.ReadLine();
#endif
            }
        }
    }
}
