using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMLM.EPayment.WebApi.ViewModels.RazerPay
{
    public class RazerPayInquiryResponseVM
    {
        public string OrderNumber { get; set; }

        public string ResponseCode { get; set; }

        public string ErrorMessage { get; set; }

        public int TransactionStatusId { get; set; }
        public string Channel { get; set; }
        public string TransactionID { get; set; }
        public string Message { get; set; }

        public RazerPayInquiryResponseVM(string orderNumber, string responseCode, string errorMessage, int transactionStatusId,
            string channel,string transactionID)
        {
            OrderNumber = orderNumber;
            ResponseCode = responseCode;
            ErrorMessage = errorMessage;
            TransactionStatusId = transactionStatusId;
            Channel = channel;
            TransactionID = transactionID;
        }
    }
}