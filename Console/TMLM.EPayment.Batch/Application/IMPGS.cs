using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Model; 

namespace TMLM.EPayment.Batch.Application
{
    interface IMPGS
    {
        BatchProcess DataFileExtration(string fullFilePath);

        bool CheckFolderExist();
        Task<bool> RunFirstTimeBatchBilling(BatchProcess batchData, List<ExtractDataModel> data);
        Task<bool> RunRecurringBatchBilling(BatchProcess batchData, List<ExtractDataModel> data);

        Task StartRestApi();

        void GenerateFile();
        void ProcessInitialFile(string filePath);
        void GenerateFlatFile(BatchProcess batchData);

        void ExtractExistingDataResponse(List<ExtractDataModel> extractDataModel);
    }
}
