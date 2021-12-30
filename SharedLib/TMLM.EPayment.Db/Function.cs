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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Dapper;
using System.Data.SqlClient;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.Db {
    public class Function {
        public static DateTime fnGetServerTime(){
            try {
                using (IDbConnection dbConn = DbConnectionFactory.OpenConnection(DBUtils.ConnectionString_Trx)) {
                    return fnGetServerTime(dbConn);
                }
            } catch {
                throw;
            } 
        }

        public static DateTime fnGetServerTime(IDbConnection dbConn) {
            DateTime dtReturn = DateTime.MinValue;
            try {
                return Convert.ToDateTime(dbConn.Query<DateTime>("SELECT GETDATE()", commandType: CommandType.Text).FirstOrDefault());
            } catch {
                throw;
            }
        }
    }
}
