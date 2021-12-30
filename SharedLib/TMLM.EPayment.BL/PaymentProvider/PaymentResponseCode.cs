using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TMLM.EPayment.BL.Data.PaymentProvider;

namespace TMLM.EPayment.BL.PaymentProvider
{
    public static class PaymentResponseCode
    {
        public const int PendingPayment = 1;
        public const int PendingAuthorization = 5;
        public const int Success = 10;
        public const int Failed = 99;
        public const int UserCancel = 98;
        public const int InvalidAmount = 97;
        public const int Timeout = 96;
        public const int InvalidSignature = 100;
        public const int TransactionNotFound = 95;
    }
}
