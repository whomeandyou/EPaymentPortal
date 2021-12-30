using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class BatchProcess
    {
        public int BatchID { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public decimal MpgsAmount { get; set; }
        public decimal TextFileAmount { get; set; }
        public int StatusId { get; set; }
        public string ProcessingStatus { get; set; }
        public bool IsRecurringBatch { get; set; }
        public string UrlGuId { get; set; }
        public string BatchData { get; set; }
        public string DataContent { get; set; }
        public virtual List<ExtractDataModel> ExtractedData { get; set; }
    }
}
