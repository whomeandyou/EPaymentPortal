/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                 
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using TMLM.Common;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.Db.Repositories
{
    public class ResponseCodeRepository : BaseRepository
    {
        public ResponseCodeRepository() : this(DBUtils.ConnectionString) { }

        public ResponseCodeRepository(string ConnStr) : base(ConnStr) { }

        public ResponseCodeRepository(IDbConnection dbConn) : base(dbConn) { }

        public ResponseCode GetResponseCodeByPaymentProviderCode(string paymentProvider, string code)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@PaymentProvider", paymentProvider, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Code", code, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<ResponseCode>("spGet_ResponseCode_By_PaymentProviderCode", _dParams,
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
