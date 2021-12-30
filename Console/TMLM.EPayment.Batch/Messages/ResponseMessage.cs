using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch
{
    public static class ResponseMessage
    {
        public static string DirectoryNotFound = "Not all directory found\nGenerating respective directory now\nAll directory generated";
        public static string DirectoryNotFoundError = "\nPlease close the program and place the files in Upload folder at ";
        public static string OperationCompleted = "All operation executed successfully, press enter to close the program";
        public static string Exception = "Exception occurred with message:\n {message} \nPress enter to close the program";
        public static string ConnectionCheck = "Establishing connection to MPGS";
        public static string Uploading = "Uploading batch to MPGS";
        public static string RetrieveResponse = "Retrieving upload response from MPGS for: {message} state";
        public static string MicValidation = "Validating MIC with MPGS";
        public static string ProcessBatch = "Retrieving processed result from MPGS\n";
        public static string ProcessBatchSuccess = "Processed response retrieval ended with success";

        public static string ReplaceMessage(string responseMessage, string message)
        {
            return responseMessage.Replace("{message}", message);
        }
    }
}
