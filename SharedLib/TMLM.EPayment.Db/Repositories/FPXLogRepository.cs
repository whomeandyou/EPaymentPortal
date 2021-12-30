using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Db.Repositories
{
    public class FPXLogRepository : BaseRepository
    {
        public FPXLogRepository() : this(DBUtils.ConnectionString) { }

        public FPXLogRepository(string ConnStr) : base(ConnStr) { }

        public FPXLogRepository(IDbConnection dbConn) : base(dbConn) { }

        public void InsertFPXLog(string orderNo, string url, string request,string response)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNo", orderNo, DbType.String, ParameterDirection.Input);
                _dParams.Add("@URL", url, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Request", request, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Response", response, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spInsert_FPXLog", _dParams, this.DbTransaction,
                  commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
