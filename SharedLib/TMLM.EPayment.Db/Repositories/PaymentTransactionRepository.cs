
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TMLM.Common;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.Db.Repositories
{
    public class PaymentTransactionRepository : BaseRepository
    {
        public PaymentTransactionRepository() : this(DBUtils.ConnectionString) { }

        public PaymentTransactionRepository(string ConnStr) : base(ConnStr) { }

        public PaymentTransactionRepository(IDbConnection dbConn) : base(dbConn) { }

        public PaymentTransaction GetPaymentTransactionByTransactionNumber(string transactionNumber)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<PaymentTransaction>("spGet_PaymentTransaction_By_TransactionNumber", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public PaymentTransaction GetPaymentTransactionByOrderNumber(string orderNumber,string paymentProvideCode)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@PaymentProviderCode", paymentProvideCode, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<PaymentTransaction>("spGet_PaymentTransaction_By_OrderNumber", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<PaymentTransaction> GetPendingPaymentTransactionListByOrderNumber(string orderNumber, string paymentProvideCode)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@PaymentProviderCode", paymentProvideCode, DbType.String, ParameterDirection.Input);

                var result = base.DbConnection.Query<PaymentTransaction>("spGet_PendingPaymentTransactionList_By_OrderNumber", _dParams, commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertPaymentTransaction(string transactionNumber, string currency, decimal amount, string orderNumber, int applicationAccountId,
             string returnUrl, string merchantId, string paymentProviderCode, bool? isEnrolment = null, bool? isInitialPayment = null,
             string additionalInfo = null, int mode = 0, string paymentRef = null, string buyerName = null, string buyerContact = null, string buyerEmail = null,
             string description = null, string cancelUrl = null)
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
                _dParams.Add("@PaymentProviderCode", paymentProviderCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Currency", currency, DbType.String, ParameterDirection.Input);
                _dParams.Add("@IsEnrolment", isEnrolment, DbType.Boolean, ParameterDirection.Input);
                _dParams.Add("@IsInitialPayment", isInitialPayment, DbType.Boolean, ParameterDirection.Input);
                _dParams.Add("@Mode", mode, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@PaymentRef", paymentRef, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatusId", 1, DbType.Boolean, ParameterDirection.Input);
                _dParams.Add("@BuyerName", buyerName, DbType.String, ParameterDirection.Input);
                _dParams.Add("@BuyerContact", buyerContact, DbType.String, ParameterDirection.Input);
                _dParams.Add("@BuyerEmail", buyerEmail, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Description", description, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CancelUrl", cancelUrl, DbType.String, ParameterDirection.Input);



                base.DbConnection.Execute("spInsert_PaymentTransaction", _dParams, this.DbTransaction,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePaymentTransaction(string transactionNumber, string paymentReferenceNumber, string bank, string responsePayload,
            string authorizationCode, string authorizationNumber, int transactionStatusId, string cardMethod = null,
            string cardType = null, string cardNumber = null, string expiryMonth = null, string expiryYear = null, string appId = null,
            string responseCode = null, string errorMessage = null, string newTransactionId = null, string channel = null)
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
                _dParams.Add("@CardMethod", cardMethod, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CardType", cardType, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CardNumber", cardNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ExpiryMonth", expiryMonth, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ExpiryYear", expiryYear, DbType.String, ParameterDirection.Input);
                _dParams.Add("@AppId", appId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ResponseCode", responseCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ErrorMessage", errorMessage, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Channel", channel, DbType.String, ParameterDirection.Input);
                _dParams.Add("@NewTransactionId", newTransactionId, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spUpdate_PaymentTransaction_TransactionStatus", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePaymentInformation(string transactionNumber, string buyerEmail, string secureId, string sessionId,
            string proposalId, bool isDifferentRenewalMethod, string msgToken)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@TransactionNumber", transactionNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@BuyerEmail", buyerEmail, DbType.String, ParameterDirection.Input);
                _dParams.Add("@SecureId", secureId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@SessionId", sessionId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ProposalId", proposalId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@IsDifferentRenewalMethod", isDifferentRenewalMethod, DbType.Boolean, ParameterDirection.Input);
                _dParams.Add("@MsgToken", msgToken, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spUpdate_PaymentTransaction_Information", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EnrollmentStatus GetEnrollmentInformation(int PaymentTransactionId)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@PaymentTransactionId", PaymentTransactionId, DbType.Int32, ParameterDirection.Input);

                return base.DbConnection.Query<EnrollmentStatus>("spGet_Enrollment_Status", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateEnrollmentInformation(int PaymentTransactionId, string Veres, string Pares,
            string AcsEci, string AuthenticationToken, string TransactionId, string dsVersion,
            string orderNumber, string transactionStatus, string statusReasonCode, string creditCardNumber, string ReceiveTransactionID)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@PaymentTransactionId", PaymentTransactionId, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@Veres", Veres, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Pares", Pares, DbType.String, ParameterDirection.Input);
                _dParams.Add("@LastModifiedOn", DateTime.UtcNow, DbType.DateTime, ParameterDirection.Input);
                _dParams.Add("@AcsEci", AcsEci, DbType.String, ParameterDirection.Input);
                _dParams.Add("@AuthenticationToken", AuthenticationToken, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionId", TransactionId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@dsVersion", dsVersion, DbType.String, ParameterDirection.Input);
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatus", transactionStatus, DbType.String, ParameterDirection.Input);
                _dParams.Add("@StatusReasonCode", statusReasonCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CreditCardNumber", creditCardNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ReceiveTransactionID", ReceiveTransactionID, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spUpdate_Enrollment_Status", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertEnrollmentInformation(int PaymentTransactionId, string Veres, string Pares,
       string AcsEci, string AuthenticationToken, string TransactionId, string dsVersion,
       string orderNumber, string transactionStatus, string statusReasonCode, string creditCardNumber, string ReceiveTransactionID)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@PaymentTransactionId", PaymentTransactionId, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@Veres", Veres, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Pares", Pares, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CreatedOn", DateTime.UtcNow, DbType.DateTime, ParameterDirection.Input);
                _dParams.Add("@LastModifiedOn", DateTime.UtcNow, DbType.DateTime, ParameterDirection.Input);
                _dParams.Add("@AcsEci", AcsEci, DbType.String, ParameterDirection.Input);
                _dParams.Add("@AuthenticationToken", AuthenticationToken, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionId", TransactionId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@dsVersion", dsVersion, DbType.String, ParameterDirection.Input);
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@TransactionStatus", transactionStatus, DbType.String, ParameterDirection.Input);
                _dParams.Add("@StatusReasonCode", statusReasonCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@CreditCardNumber", creditCardNumber, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ReceiveTransactionID", ReceiveTransactionID, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spInsert_Enrollment_Status", _dParams,
                   commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PaymentTransaction SpGetPaymentInformation(string orderNum, string transactionNum)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNum", orderNum, DbType.String, ParameterDirection.Input);
                _dParams.Add("@@TransactionNum", transactionNum, DbType.String, ParameterDirection.Input);


                return base.DbConnection.Query<PaymentTransaction>("spGet_PaymentTransaction_With_OrderNum_And_TransactionNum", _dParams,
                   commandType: CommandType.StoredProcedure)
                   .FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PaymentTransaction GetPaymentTransactionByOrdernumberSuccess(string orderNumber)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNumber", orderNumber, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<PaymentTransaction>("spGet_PaymentTransaction_By_OrderNumber_MPGS_Success", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ApplicationAccount SpGetApplicationAccount(string code)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@Code", code, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<ApplicationAccount>("spGet_ApplicationAccount_By_Code", _dParams,
                   commandType: CommandType.StoredProcedure)
                   .FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SpUpdateInquiryReturnUrl(string orderId, string returnUrl)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderId", orderId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ReturnUrl", returnUrl, DbType.String, ParameterDirection.Input);

                await base.DbConnection.ExecuteAsync("spUpdate_InquiryReturnUrl", _dParams, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SpUpdateStatus(string orderId, int statusId, string responseCode, string errorMessage, string statCode,
            string channel,string transID)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderId", orderId, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Status", statusId, DbType.Int32, ParameterDirection.Input);
                _dParams.Add("@responseCode", responseCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@errorMessage", errorMessage, DbType.String, ParameterDirection.Input);
                _dParams.Add("@statCode", statCode, DbType.String, ParameterDirection.Input);
                _dParams.Add("@channel", channel, DbType.String, ParameterDirection.Input);
                _dParams.Add("@transID", transID, DbType.String, ParameterDirection.Input);

                await base.DbConnection.ExecuteAsync("spUpdate_Status", _dParams, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}
