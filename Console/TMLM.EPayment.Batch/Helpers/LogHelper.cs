using log4net;
using System;

namespace TMLM.EPayment.Batch.Helpers
{
    public static class LogHelper
    {
        private static readonly ILog logger = LogManager.GetLogger("EPaymentMPGSBatch");

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Warn(string message)
        {
            logger.Warn(message);
        }

        public static void WarnFormat(string message, object obj)
        {
            logger.WarnFormat(message, obj);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        public static void ErrorFormat(string message, object obj)
        {
            logger.ErrorFormat(message, obj);
        }
    }
}
