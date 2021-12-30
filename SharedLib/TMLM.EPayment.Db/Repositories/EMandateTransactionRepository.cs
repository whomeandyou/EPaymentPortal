using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.Db.Repositories
{
    public class EMandateTransactionRepository : BaseRepository
    {
        public EMandateTransactionRepository() : this(DBUtils.ConnectionString) { }

        public EMandateTransactionRepository(string ConnStr) : base(ConnStr) { }

        public EMandateTransactionRepository(IDbConnection dbConn) : base(dbConn) { }
        public EMandateTransaction GetPaymentTransactionByTransactionNumber(string transactionNumber)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<EMandateTransaction>("spGet_EMandateTransaction_By_TransactionNumber", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public EMandateTransaction GetPaymentTransactionByOrderNumber(string orderNumber)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<EMandateTransaction>("spGet_EMandateTransaction_By_OrderNumber", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void InsertEMandateTransaction(string transactionNumber, string currency, decimal amount, string orderNumber, int applicationAccountId,
            string returnUrl, string merchantId, string idType, string idNo, string applicationType, int maxFrequency, string frequencyMode, string buyerEmail,
            string name, string description, string mobilePhoneNo, string paymentRef, string msgToken = "01", int mode = 0)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@ApplicationAccountId", applicationAccountId, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@Amount", amount, DbType.Decimal, ParameterDirection.Input);
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ReturnUrl", returnUrl, DbType.String, ParameterDirection.Input);
                _dParams.Add("@MerchantId", merchantId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Currency", currency, DbType.String, ParameterDirection.Input);
                _dParams.Add("@IdType", idType, DbType.String, ParameterDirection.Input);
                _dParams.Add("@IdNo", idNo, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ApplicationType", applicationType, DbType.String, ParameterDirection.Input);
                _dParams.Add("@MaxFrequency", maxFrequency, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@FrequencyMode", frequencyMode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@BuyerEmail", buyerEmail, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Name", name, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Desc", description, DbType.String, ParameterDirection.Input);
                _dParams.Add("@MobileNo", mobilePhoneNo, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Mode", mode, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@PaymentRef", paymentRef, DbType.String, ParameterDirection.Input);
                _dParams.Add("@MsgToken", msgToken, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatusId", 1, DbType.Int32, ParameterDirection.Input);

                base.DbConnection.Execute("spInsert_EMandateTransaction", _dParams, this.DbTransaction,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePaymentTransaction(string transactionNumber, string paymentReferenceNumber, string bank, string responsePayload,
            string authorizationCode, string authorizationNumber, int transactionStatusId, decimal? txnAmount = null)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@PaymentReferenceNumber", paymentReferenceNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Bank", bank, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ResponsePayload", responsePayload, DbType.String, ParameterDirection.Input);
                _dParams.Add("@AuthorizationCode", authorizationCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatusId", transactionStatusId, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@AuthorizationNumber", authorizationNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Amount", txnAmount, DbType.Decimal, ParameterDirection.Input);

                base.DbConnection.Execute("spUpdate_EMandateTransaction_TransactionStatus", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateEMandateInformation(string transactionNumber, string bankCode, int status=0)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@BankCode", bankCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatusId", status, DbType.Int32, ParameterDirection.Input);

                base.DbConnection.Execute("spUpdate_EMandateTransaction_Info", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
