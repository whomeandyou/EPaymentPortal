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
    public class StoreProcedure {
        public class Reporting {

            //[Obsolete("This method was obslute and split into sprpt_dashboard_memberstatistic_byage, sprpt_dashboard_memberstatistic_bygender, sprpt_dashboard_memberstatistic_byrace, sprpt_dashboard_memberstatistic_bysocialtype")]
            //public static List<spRpt_Dashboard_Table> spRpt_Dashboard(string Chart_Type, out string retCode, out string retMsg) {
            //    using (IDbConnection dbConn = DbConnectionFactory.OpenConnection(DBUtils.ConnectionString_Rpt)) {
            //        List<spRpt_Dashboard_Table> tblReturn =
            //            Reporting.spRpt_Dashboard(dbConn, Chart_Type, out retCode, out retMsg);
            //        return tblReturn;
            //    }
            //}
            #region Rpt_Dashboard_VisitOverview
            #endregion


        }

    }
}
