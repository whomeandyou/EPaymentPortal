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
using System.Data.SqlClient;

namespace TMLM.EPayment.Db {
    public class DbConnectionFactory {
        public static IDbConnection OpenConnection(string ConnString) {
            IDbConnection connection = new SqlConnection(ConnString);
            connection.Open();
            return connection;
        }
    }
}
