using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class EMandateTransactionService : IDisposable
    {
        bool disposed = false;

        public EMandateTransactionService() { }

        public EMandateTransaction GetPaymentTransactionByTransactionNumber(string transactionNumber)
        {
            using (var repoPaymentTransaction = new EMandateTransactionRepository())
                return repoPaymentTransaction.GetPaymentTransactionByTransactionNumber(transactionNumber);
        }
        public EMandateTransaction GetPaymentTransactionByOrderNumber(string orderNumber, string paymentProvideCode)
        {
            using (var repoPaymentTransaction = new EMandateTransactionRepository())
                return repoPaymentTransaction.GetPaymentTransactionByOrderNumber(orderNumber);
        }
        public void UpdateEMandateInformation(UpdatePaymentInformationInputModel model)
        {
            using (var repoPaymentTransaction = new EMandateTransactionRepository())
            {
                repoPaymentTransaction.UpdateEMandateInformation(model.TransactionNumber, model.BuyerBank, model.Status);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            // Free any unmanaged objects here.
            if (disposing)
            {
                //stuff to dispose
            }
            disposed = true;
        }
    }
}
