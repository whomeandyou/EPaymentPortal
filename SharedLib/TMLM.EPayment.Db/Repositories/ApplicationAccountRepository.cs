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
    public class ApplicationAccountRepository : BaseRepository
    {
        public ApplicationAccountRepository() : this(DBUtils.ConnectionString) { }

        public ApplicationAccountRepository(string ConnStr) : base(ConnStr) { }

        public ApplicationAccountRepository(IDbConnection dbConn) : base(dbConn) { }

        public ApplicationAccount GetApplicationAccountByCode(string code)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@Code", code, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<ApplicationAccount>("spGet_ApplicationAccount_By_Code", _dParams,
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ApplicationAccount GetApplicationAccountById(int id)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@Id", id, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<ApplicationAccount>("spGet_ApplicationAccount_By_Id", _dParams,
                    commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }


}
