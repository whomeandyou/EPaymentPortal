using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;

namespace TMLM.EPayment.Db.Repositories
{
    public class RazerPayLogRepository : BaseRepository
    {
        public RazerPayLogRepository() : this(DBUtils.ConnectionString) { }

        public RazerPayLogRepository(string ConnStr) : base(ConnStr) { }

        public RazerPayLogRepository(IDbConnection dbConn) : base(dbConn) { }

        public void InsertRazerPayLog(string orderNo, string url, string request, string response)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNo", orderNo, DbType.String, ParameterDirection.Input);
                _dParams.Add("@URL", url, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Request", request, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Response", response, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spInsert_RazerPayLog", _dParams, this.DbTransaction,
                  commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
