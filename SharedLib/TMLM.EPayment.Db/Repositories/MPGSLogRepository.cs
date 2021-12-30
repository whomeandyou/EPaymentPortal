using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Db.Repositories
{
    public class MPGSLogRepository : BaseRepository
    {
        public MPGSLogRepository() : this(DBUtils.ConnectionString) { }

        public MPGSLogRepository(string ConnStr) : base(ConnStr) { }

        public MPGSLogRepository(IDbConnection dbConn) : base(dbConn) { }

        public void InsertMPGSLog(string orderNo,string apiOperation,string request,string response)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@OrderNo", orderNo, DbType.String, ParameterDirection.Input);
                _dParams.Add("@ApiOperation", apiOperation, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Request", request, DbType.String, ParameterDirection.Input);
                _dParams.Add("@Response", response, DbType.String, ParameterDirection.Input);

                base.DbConnection.Execute("spInsert_MPGSLog", _dParams, this.DbTransaction,
                  commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
