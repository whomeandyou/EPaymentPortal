
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.IO;
using System.Transactions;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TMLM;
using TMLM.Common;
using System.Security.Cryptography;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;
using TMLM.EPayment.BL.Data.PaymentProvider;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class PaymentTransactionService : IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        public PaymentTransactionService() { }

        public PaymentTransaction GetPaymentTransactionByTransactionNumber(string transactionNumber)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
                return repoPaymentTransaction.GetPaymentTransactionByTransactionNumber(transactionNumber);
        }

        public PaymentTransaction GetPaymentTransactionByOrderNumber(string orderNumber,string paymentProvideCode)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
                return repoPaymentTransaction.GetPaymentTransactionByOrderNumber(orderNumber,paymentProvideCode);
        }

        public List<PaymentTransaction> GetPendingPaymentTransactionListByOrderNumber(string orderNumber, string paymentProvideCode)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
                return repoPaymentTransaction.GetPendingPaymentTransactionListByOrderNumber(orderNumber, paymentProvideCode);
        }

        public PaymentTransaction GetPaymentTransactionByOrderNumberSuccessful(string orderNumber)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
                return repoPaymentTransaction.GetPaymentTransactionByOrdernumberSuccess(orderNumber);
        }

        public void UpdatePaymentInformation(UpdatePaymentInformationInputModel model)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
            {
                repoPaymentTransaction.UpdatePaymentInformation(model.TransactionNumber, model.BuyerEmail, model.SecureId,
                   model.SessionId, model.ProposalId, model.IsDifferentRenewalMethod, model.MsgToken);
            }
        }

        public EnrollmentStatus GetEnrollmentInformation(int Id)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
            {
               return repoPaymentTransaction.GetEnrollmentInformation(Id);
            }
        }

        public void InsertEnrollmentInformation(EnrollmentInformationModel model)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
            {
                repoPaymentTransaction.InsertEnrollmentInformation(model.PaymentTransactionId, model.Veres, model.Pares,
                    model.AcsEci, model.AuthenticationToken, model.TransactionId, model.dsVersion,
                    model.OrderNumber,model.TransactionStatus,model.StatusReasonCode,model.CreditCardNumber,model.ReceiveTransactionID);
            }
        }

        public void UpdateEnrollmentInformation(EnrollmentInformationModel model)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
            {
                repoPaymentTransaction.UpdateEnrollmentInformation(model.PaymentTransactionId, model.Veres, model.Pares,
                    model.AcsEci, model.AuthenticationToken, model.TransactionId, model.dsVersion,
                    model.OrderNumber, model.TransactionStatus, model.StatusReasonCode, model.CreditCardNumber, model.ReceiveTransactionID);
            }
        }

        public PaymentTransaction GetPaymentInformationByOrderIdAndTransactioNum(string orderNumber, string transactionNum)
        {
            using (var repoPaymentTransaction = new PaymentTransactionRepository())
                return repoPaymentTransaction.SpGetPaymentInformation(orderNumber, transactionNum);
            
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
